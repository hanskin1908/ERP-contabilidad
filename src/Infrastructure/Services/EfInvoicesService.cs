using Application.Dtos;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class EfInvoicesService(ApplicationDbContext db) : IInvoicesService
{
    private const long DefaultCompanyId = 1;

    public async Task<IReadOnlyList<InvoiceDto>> SearchAsync(string? type, string? status, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var q = db.Invoices.AsNoTracking().Where(i => i.CompanyId == DefaultCompanyId);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(i => i.Type == type);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(i => i.Status == status);
        if (from is not null) q = q.Where(i => i.IssueDate >= from);
        if (to is not null) q = q.Where(i => i.IssueDate <= to);

        var list = await q.OrderByDescending(i => i.IssueDate).ThenByDescending(i => i.Number)
            .Select(i => new InvoiceDto(i.Id, i.Type, i.Number, i.ThirdPartyId, i.IssueDate, i.DueDate, i.Currency, i.Subtotal, i.TaxTotal, i.Total, i.Status, i.JournalEntryId, i.Notes ?? "", new()))
            .ToListAsync(ct);
        return list;
    }

    public async Task<InvoiceDto?> GetAsync(long id, CancellationToken ct)
    {
        var i = await db.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (i is null) return null;
        var lines = await db.InvoiceLines.AsNoTracking().Where(l => l.InvoiceId == id).OrderBy(l => l.Id)
            .Select(l => new InvoiceLineDto(l.Id, l.ItemName, l.Quantity, l.UnitPrice, l.Discount, l.TaxRate, l.AccountId, l.Total))
            .ToListAsync(ct);
        return new InvoiceDto(i.Id, i.Type, i.Number, i.ThirdPartyId, i.IssueDate, i.DueDate, i.Currency, i.Subtotal, i.TaxTotal, i.Total, i.Status, i.JournalEntryId, i.Notes, lines);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest req, CancellationToken ct)
    {
        await ValidateThirdAsync(req.ThirdPartyId, ct);
        await ValidateLineAccountsAsync(req.Lines.Select(l => l.AccountId).Distinct().ToArray(), ct);

        var (sub, tax, total) = ComputeTotals(req.Lines);
        var number = await NextConsecutiveAsync(DefaultCompanyId, req.Type, ct);
        var inv = new Invoice
        {
            CompanyId = DefaultCompanyId,
            Type = req.Type,
            Number = number,
            ThirdPartyId = req.ThirdPartyId,
            IssueDate = req.IssueDate,
            DueDate = req.DueDate,
            Currency = req.Currency,
            Subtotal = sub,
            TaxTotal = tax,
            Total = total,
            Status = "DRAFT",
            Notes = req.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Invoices.Add(inv);
        await db.SaveChangesAsync(ct);

        foreach (var l in req.Lines)
        {
            db.InvoiceLines.Add(new InvoiceLine
            {
                InvoiceId = inv.Id,
                ItemName = l.ItemName,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Discount = l.Discount,
                TaxRate = l.TaxRate,
                AccountId = l.AccountId,
                Total = LineTotal(l)
            });
        }
        await db.SaveChangesAsync(ct);
        return (await GetAsync(inv.Id, ct))!;
    }

    public async Task<InvoiceDto?> UpdateAsync(long id, UpdateInvoiceRequest req, CancellationToken ct)
    {
        var inv = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (inv is null) return null;
        if (inv.Status != "DRAFT") throw new InvalidOperationException("Solo DRAFT puede editarse");
        await ValidateLineAccountsAsync(req.Lines.Select(l => l.AccountId).Distinct().ToArray(), ct);

        var (sub, tax, total) = ComputeTotals(req.Lines);
        inv.IssueDate = req.IssueDate;
        inv.DueDate = req.DueDate;
        inv.Currency = req.Currency;
        inv.Subtotal = sub;
        inv.TaxTotal = tax;
        inv.Total = total;
        inv.Notes = req.Notes;
        inv.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var old = await db.InvoiceLines.Where(l => l.InvoiceId == inv.Id).ToListAsync(ct);
        db.InvoiceLines.RemoveRange(old);
        await db.SaveChangesAsync(ct);
        foreach (var l in req.Lines)
        {
            db.InvoiceLines.Add(new InvoiceLine
            {
                InvoiceId = inv.Id,
                ItemName = l.ItemName,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Discount = l.Discount,
                TaxRate = l.TaxRate,
                AccountId = l.AccountId,
                Total = LineTotal(l)
            });
        }
        await db.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task<bool> ApproveAsync(long id, ApproveInvoiceRequest req, CancellationToken ct)
    {
        var inv = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (inv is null) return false;
        if (inv.Status != "DRAFT") throw new InvalidOperationException("Solo DRAFT puede aprobarse");

        // Obtener configuración de cuentas por defecto
        var cfg = await db.CompanyConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyId == DefaultCompanyId, ct);
        long? counterAccount = req.CounterAccountId;
        if (counterAccount is null)
            counterAccount = inv.Type == "SALE" ? cfg?.CxcAccountId : cfg?.CxpAccountId;
        if (counterAccount is null) throw new InvalidOperationException("Cuenta contrapartida no configurada. Configure CxC/CxP en la empresa o envíe 'counterAccountId'.");
        await ValidateAccountPostableAsync(counterAccount.Value, ct);
        await ValidateThirdAsync(inv.ThirdPartyId, ct);

        var lines = await db.InvoiceLines.Where(l => l.InvoiceId == inv.Id).ToListAsync(ct);
        var accountIds = lines.Select(l => l.AccountId).Distinct().ToArray();
        await ValidateLineAccountsAsync(accountIds, ct);

        var date = req.PostingDate ?? inv.IssueDate;
        var type = inv.Type == "SALE" ? "INGRESO" : "EGRESO";
        var number = await NextJournalNumberAsync(DefaultCompanyId, type, ct);

        var je = new JournalEntry
        {
            CompanyId = DefaultCompanyId,
            Number = number,
            Type = type,
            Date = date,
            Description = $"Aprobación factura {inv.Type}-{inv.Number}",
            ThirdPartyId = inv.ThirdPartyId,
            Status = "DRAFT",
            TotalDebit = 0,
            TotalCredit = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.JournalEntries.Add(je);
        await db.SaveChangesAsync(ct);

        decimal totalDebit = 0, totalCredit = 0;
        decimal totalTax = 0;
        if (inv.Type == "SALE")
        {
            // Debit CxC por total
            db.JournalLines.Add(new JournalLine { JournalEntryId = je.Id, AccountId = counterAccount.Value, Description = "CxC", Debit = inv.Total, Credit = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            totalDebit += inv.Total;
            foreach (var l in lines)
            {
                var baseV = (l.Quantity * l.UnitPrice) - l.Discount;
                var tax = Math.Round((double)(baseV * (l.TaxRate / 100m)), 2);
                totalTax += (decimal)tax;
                db.JournalLines.Add(new JournalLine { JournalEntryId = je.Id, AccountId = l.AccountId, Description = l.ItemName, Debit = 0, Credit = decimal.Round(baseV,2), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
                totalCredit += decimal.Round(baseV,2);
            }
            if (totalTax > 0)
            {
            var ivaAcc = cfg?.IvaVentasAccountId ?? throw new InvalidOperationException("Cuenta de IVA ventas no configurada");
            await ValidateAccountPostableAsync(ivaAcc, ct);
            db.JournalLines.Add(new JournalLine { JournalEntryId = je.Id, AccountId = ivaAcc, Description = "IVA Ventas", Debit = 0, Credit = decimal.Round(totalTax,2), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
                totalCredit += decimal.Round(totalTax,2);
            }
        }
        else // PURCHASE
        {
            // Credit CxP por total
            db.JournalLines.Add(new JournalLine { JournalEntryId = je.Id, AccountId = counterAccount.Value, Description = "CxP", Debit = 0, Credit = inv.Total, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            totalCredit += inv.Total;
            foreach (var l in lines)
            {
                var baseV = (l.Quantity * l.UnitPrice) - l.Discount;
                var tax = Math.Round((double)(baseV * (l.TaxRate / 100m)), 2);
                totalTax += (decimal)tax;
                db.JournalLines.Add(new JournalLine { JournalEntryId = je.Id, AccountId = l.AccountId, Description = l.ItemName, Debit = decimal.Round(baseV,2), Credit = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
                totalDebit += decimal.Round(baseV,2);
            }
            if (totalTax > 0)
            {
            var ivaAcc = cfg?.IvaComprasAccountId ?? throw new InvalidOperationException("Cuenta de IVA compras no configurada");
            await ValidateAccountPostableAsync(ivaAcc, ct);
            db.JournalLines.Add(new JournalLine { JournalEntryId = je.Id, AccountId = ivaAcc, Description = "IVA Compras", Debit = decimal.Round(totalTax,2), Credit = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
                totalDebit += decimal.Round(totalTax,2);
            }
        }

        if (totalDebit != totalCredit) throw new InvalidOperationException("Asiento generado no balanceado");
        je.TotalDebit = totalDebit; je.TotalCredit = totalCredit; je.Status = "POSTED";
        await db.SaveChangesAsync(ct);

        inv.Status = "APPROVED"; inv.JournalEntryId = je.Id; inv.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CancelAsync(long id, CancellationToken ct)
    {
        var inv = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (inv is null) return false;
        if (inv.Status != "DRAFT") throw new InvalidOperationException("Solo DRAFT puede cancelarse");
        inv.Status = "CANCELLED"; inv.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static (decimal sub, decimal tax, decimal total) ComputeTotals(IEnumerable<CreateInvoiceLineRequest> lines)
    {
        decimal sub = 0, tax = 0;
        foreach (var l in lines)
        {
            var lt = LineTotal(l);
            sub += (l.Quantity * l.UnitPrice) - l.Discount;
            tax += decimal.Round(((l.Quantity * l.UnitPrice) - l.Discount) * (l.TaxRate / 100m), 2);
        }
        var total = sub + tax;
        return (decimal.Round(sub, 2), decimal.Round(tax, 2), decimal.Round(total, 2));
    }

    private static decimal LineTotal(CreateInvoiceLineRequest l)
    {
        var baseV = (l.Quantity * l.UnitPrice) - l.Discount;
        var t = decimal.Round(baseV * (l.TaxRate / 100m), 2);
        return decimal.Round(baseV + t, 2);
    }

    private async Task ValidateLineAccountsAsync(long[] accountIds, CancellationToken ct)
    {
        var dict = await db.Accounts.AsNoTracking().Where(a => a.CompanyId == DefaultCompanyId && accountIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, ct);
        foreach (var id in accountIds)
        {
            if (!dict.TryGetValue(id, out var acc)) throw new InvalidOperationException($"Cuenta {id} no existe");
            if (!acc.IsPostable) throw new InvalidOperationException($"Cuenta {acc.Code} no es posteable");
        }
    }

    private async Task ValidateAccountPostableAsync(long? accountId, CancellationToken ct)
    {
        if (accountId is null) throw new InvalidOperationException("Cuenta requerida");
        var acc = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.CompanyId == DefaultCompanyId && a.Id == accountId.Value, ct) ?? throw new InvalidOperationException("Cuenta no existe");
        if (!acc.IsPostable) throw new InvalidOperationException($"Cuenta {acc.Code} no es posteable");
    }

    private async Task ValidateThirdAsync(long thirdId, CancellationToken ct)
    {
        var exists = await db.ThirdParties.AnyAsync(t => t.CompanyId == DefaultCompanyId && t.Id == thirdId, ct);
        if (!exists) throw new InvalidOperationException($"Tercero {thirdId} no existe");
    }

    private async Task<long> NextConsecutiveAsync(long companyId, string type, CancellationToken ct)
    {
        var max = await db.Invoices.Where(i => i.CompanyId == companyId && i.Type == type).Select(i => (long?)i.Number).MaxAsync(ct);
        return (max ?? 0) + 1;
    }

    private async Task<long> NextJournalNumberAsync(long companyId, string type, CancellationToken ct)
    {
        var max = await db.JournalEntries.Where(j => j.CompanyId == companyId && j.Type == type).Select(j => (long?)j.Number).MaxAsync(ct);
        return (max ?? 0) + 1;
    }
}
