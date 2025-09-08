using Application.Dtos;
using Application.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class EfJournalService(ApplicationDbContext db) : IJournalService
{
    private const long DefaultCompanyId = 1;

    public async Task<IReadOnlyList<JournalEntrySummaryDto>> SearchAsync(DateOnly? from, DateOnly? to, string? type, long? thirdPartyId, string? q, CancellationToken ct)
    {
        var query = db.JournalEntries.AsNoTracking().Where(j => j.CompanyId == DefaultCompanyId);
        if (from is not null) query = query.Where(j => j.Date >= from);
        if (to is not null) query = query.Where(j => j.Date <= to);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(j => j.Type == type);
        if (thirdPartyId is not null) query = query.Where(j => j.ThirdPartyId == thirdPartyId);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(j => (j.Description ?? "").Contains(s));
        }
        var list = await query
            .OrderByDescending(j => j.Date).ThenByDescending(j => j.Number)
            .Select(j => new JournalEntrySummaryDto(j.Id, j.Number, j.Type, j.Date, j.Status, j.TotalDebit, j.TotalCredit))
            .ToListAsync(ct);
        return list;
    }

    public async Task<JournalEntryDto?> GetAsync(long id, CancellationToken ct)
    {
        var j = await db.JournalEntries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (j is null) return null;
        var lines = await db.JournalLines.AsNoTracking()
            .Where(l => l.JournalEntryId == j.Id)
            .OrderBy(l => l.Id)
            .Select(l => new JournalLineDto(l.Id, l.AccountId, l.Description, l.Category, l.Debit, l.Credit, l.ThirdPartyId))
            .ToListAsync(ct);
        return new JournalEntryDto(j.Id, j.Number, j.Type, j.Date, j.Description, j.Status, j.TotalDebit, j.TotalCredit, lines);
    }

    public async Task<JournalEntryDto> CreateDraftAsync(CreateJournalEntryRequest req, CancellationToken ct)
    {
        ValidateLines(req.Lines);

        // Validar cuentas posteables y existencia
        var accountIds = req.Lines.Select(l => l.AccountId).Distinct().ToArray();
        var accounts = await db.Accounts.AsNoTracking()
            .Where(a => a.CompanyId == DefaultCompanyId && accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);
        foreach (var id in accountIds)
        {
            if (!accounts.TryGetValue(id, out var acc))
                throw new InvalidOperationException($"Cuenta {id} no existe");
            if (!acc.IsPostable)
                throw new InvalidOperationException($"Cuenta {acc.Code} no es posteable");
        }

        // Validar terceros (header y líneas) si se envían
        if (req.ThirdPartyId is not null)
        {
            var existsTp = await db.ThirdParties.AnyAsync(t => t.CompanyId == DefaultCompanyId && t.Id == req.ThirdPartyId, ct);
            if (!existsTp) throw new InvalidOperationException($"Tercero {req.ThirdPartyId} no existe");
        }
        var lineThirds = req.Lines.Select(l => l.ThirdPartyId).Where(id => id is not null).Cast<long>().Distinct().ToArray();
        if (lineThirds.Length > 0)
        {
            var count = await db.ThirdParties.CountAsync(t => t.CompanyId == DefaultCompanyId && lineThirds.Contains(t.Id), ct);
            if (count != lineThirds.Length) throw new InvalidOperationException("Algún tercero en líneas no existe");
        }

        var number = await NextConsecutiveAsync(DefaultCompanyId, req.Type, ct);
        var (debit, credit) = SumLines(req.Lines);
        if (debit != credit)
            throw new InvalidOperationException("Débitos y créditos no balancean");

        var entry = new Domain.Entities.JournalEntry
        {
            CompanyId = DefaultCompanyId,
            Number = number,
            Type = req.Type,
            Date = req.Date,
            Description = req.Description,
            ThirdPartyId = req.ThirdPartyId,
            Status = "DRAFT",
            TotalDebit = debit,
            TotalCredit = credit,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.JournalEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        foreach (var l in req.Lines)
        {
            db.JournalLines.Add(new Domain.Entities.JournalLine
            {
                JournalEntryId = entry.Id,
                AccountId = l.AccountId,
                Description = l.Description,
                Category = l.Category,
                Debit = l.Debit,
                Credit = l.Credit,
                ThirdPartyId = l.ThirdPartyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }
        await db.SaveChangesAsync(ct);
        return await GetAsync(entry.Id, ct) ?? throw new InvalidOperationException("Error creando asiento");
    }

    public async Task<JournalEntryDto?> UpdateDraftAsync(long id, UpdateJournalEntryRequest req, CancellationToken ct)
    {
        var entry = await db.JournalEntries.FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == DefaultCompanyId, ct);
        if (entry is null) return null;
        if (entry.Status != "DRAFT") throw new InvalidOperationException("Solo se puede editar si está en DRAFT");

        ValidateLines(req.Lines);
        var accountIds = req.Lines.Select(l => l.AccountId).Distinct().ToArray();
        var accounts = await db.Accounts.AsNoTracking()
            .Where(a => a.CompanyId == DefaultCompanyId && accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, ct);
        foreach (var idAcc in accountIds)
        {
            if (!accounts.TryGetValue(idAcc, out var acc))
                throw new InvalidOperationException($"Cuenta {idAcc} no existe");
            if (!acc.IsPostable)
                throw new InvalidOperationException($"Cuenta {acc.Code} no es posteable");
        }
        // Validar terceros
        if (req.ThirdPartyId is not null)
        {
            var existsTp = await db.ThirdParties.AnyAsync(t => t.CompanyId == DefaultCompanyId && t.Id == req.ThirdPartyId, ct);
            if (!existsTp) throw new InvalidOperationException($"Tercero {req.ThirdPartyId} no existe");
        }
        var lineThirds = req.Lines.Select(l => l.ThirdPartyId).Where(id => id is not null).Cast<long>().Distinct().ToArray();
        if (lineThirds.Length > 0)
        {
            var count = await db.ThirdParties.CountAsync(t => t.CompanyId == DefaultCompanyId && lineThirds.Contains(t.Id), ct);
            if (count != lineThirds.Length) throw new InvalidOperationException("Algún tercero en líneas no existe");
        }

        var (debit, credit) = SumLines(req.Lines);
        if (debit != credit)
            throw new InvalidOperationException("Débitos y créditos no balancean");

        entry.Date = req.Date;
        entry.Description = req.Description;
        entry.ThirdPartyId = req.ThirdPartyId;
        entry.TotalDebit = debit;
        entry.TotalCredit = credit;
        entry.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        // Reemplazar líneas
        var existing = await db.JournalLines.Where(l => l.JournalEntryId == entry.Id).ToListAsync(ct);
        db.JournalLines.RemoveRange(existing);
        await db.SaveChangesAsync(ct);
        foreach (var l in req.Lines)
        {
            db.JournalLines.Add(new Domain.Entities.JournalLine
            {
                JournalEntryId = entry.Id,
                AccountId = l.AccountId,
                Description = l.Description,
                Category = l.Category,
                Debit = l.Debit,
                Credit = l.Credit,
                ThirdPartyId = l.ThirdPartyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }
        await db.SaveChangesAsync(ct);
        return await GetAsync(entry.Id, ct);
    }

    public async Task<bool> PostAsync(long id, CancellationToken ct)
    {
        var entry = await db.JournalEntries.FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == DefaultCompanyId, ct);
        if (entry is null) return false;
        if (entry.Status != "DRAFT") throw new InvalidOperationException("Solo DRAFT puede publicarse");
        if (entry.TotalDebit != entry.TotalCredit) throw new InvalidOperationException("Asiento no balanceado");
        entry.Status = "POSTED";
        entry.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> VoidAsync(long id, CancellationToken ct)
    {
        var entry = await db.JournalEntries.FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == DefaultCompanyId, ct);
        if (entry is null) return false;
        if (entry.Status == "VOID") return true;
        // Política simple MVP: marcar como VOID (sin asiento reverso automático)
        entry.Status = "VOID";
        entry.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static void ValidateLines(IReadOnlyCollection<CreateJournalLineRequest> lines)
    {
        if (lines.Count == 0) throw new InvalidOperationException("Debe incluir al menos una línea");
        foreach (var l in lines)
        {
            if (l.Debit < 0 || l.Credit < 0) throw new InvalidOperationException("Débito/Crédito no pueden ser negativos");
            if (l.Debit == 0 && l.Credit == 0) throw new InvalidOperationException("Débito y Crédito no pueden ser ambos 0");
        }
    }

    private static (decimal debit, decimal credit) SumLines(IEnumerable<CreateJournalLineRequest> lines)
    {
        decimal d = 0, c = 0;
        foreach (var l in lines) { d += l.Debit; c += l.Credit; }
        return (decimal.Round(d, 2), decimal.Round(c, 2));
    }

    private async Task<long> NextConsecutiveAsync(long companyId, string type, CancellationToken ct)
    {
        var max = await db.JournalEntries.Where(j => j.CompanyId == companyId && j.Type == type)
            .Select(j => (long?)j.Number).MaxAsync(ct);
        return (max ?? 0) + 1;
    }
}
