using Application.Dtos;
using Application.Services;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class EfReportsService(ApplicationDbContext db) : IReportsService
{
    private const long DefaultCompanyId = 1;
    private record AccLine(string Code, char Nature, decimal Debit, decimal Credit);

    public async Task<IncomeStatementDto> GetIncomeStatementAsync(DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        async Task<decimal> SumRangePrefix(string prefix)
        {
            var q = from l in db.JournalLines
                    join e in db.JournalEntries on l.JournalEntryId equals e.Id
                    join a in db.Accounts on l.AccountId equals a.Id
                    where e.CompanyId == DefaultCompanyId
                          && e.Status == "POSTED"
                          && e.Date >= fromDate && e.Date <= toDate
                          && a.Code.StartsWith(prefix)
                    select new { a.Nature, l.Debit, l.Credit };
            return await q.SumAsync(x => x.Nature == 'D' ? x.Debit - x.Credit : x.Credit - x.Debit, ct);
        }

        decimal ingresos = await SumRangePrefix("4");
        decimal gastos = await SumRangePrefix("5");
        decimal costos6 = await SumRangePrefix("6");
        decimal costos7 = await SumRangePrefix("7");
        decimal costos = costos6 + costos7;
        decimal utilidad = ingresos - costos - gastos;
        return new IncomeStatementDto(fromDate, toDate, Round(ingresos), Round(costos), Round(gastos), Round(utilidad));
    }

    public async Task<BalanceSheetDto> GetBalanceSheetAsync(DateOnly asOf, CancellationToken ct)
    {
        async Task<decimal> SumToDatePrefix(string prefix)
        {
            var q = from l in db.JournalLines
                    join e in db.JournalEntries on l.JournalEntryId equals e.Id
                    join a in db.Accounts on l.AccountId equals a.Id
                    where e.CompanyId == DefaultCompanyId
                          && e.Status == "POSTED"
                          && e.Date <= asOf
                          && a.Code.StartsWith(prefix)
                    select new { a.Nature, l.Debit, l.Credit };
            return await q.SumAsync(x => x.Nature == 'D' ? x.Debit - x.Credit : x.Credit - x.Debit, ct);
        }

        decimal activos = await SumToDatePrefix("1");
        decimal pasivos = await SumToDatePrefix("2");
        decimal patrimonio = await SumToDatePrefix("3");
        return new BalanceSheetDto(asOf, Round(activos), Round(pasivos), Round(patrimonio));
    }

    // SumByPrefix removed in favor of direct queries to avoid provider translation issues

    private static decimal Round(decimal v) => decimal.Round(v, 2);

    public async Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalanceAsync(DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        var q = from l in db.JournalLines
                join e in db.JournalEntries on l.JournalEntryId equals e.Id
                join a in db.Accounts on l.AccountId equals a.Id
                where e.CompanyId == DefaultCompanyId && e.Status == "POSTED" && e.Date >= fromDate && e.Date <= toDate
                select new { a.Code, a.Name, a.Nature, l.Debit, l.Credit };
        var rows = await q.GroupBy(x => new { x.Code, x.Name, x.Nature })
            .Select(g => new
            {
                g.Key.Code,
                g.Key.Name,
                Debits = g.Sum(x => x.Debit),
                Credits = g.Sum(x => x.Credit),
                Nature = g.Key.Nature
            })
            .OrderBy(x => x.Code)
            .ToListAsync(ct);
        return rows.Select(r => new TrialBalanceRowDto(r.Code, r.Name, Round(r.Debits), Round(r.Credits), Round(r.Nature == 'D' ? r.Debits - r.Credits : r.Credits - r.Debits))).ToList();
    }

    public async Task<IReadOnlyList<JournalRowDto>> GetJournalAsync(DateOnly fromDate, DateOnly toDate, string? type, long? thirdPartyId, string? category, CancellationToken ct)
    {
        var q = from l in db.JournalLines
                join e in db.JournalEntries on l.JournalEntryId equals e.Id
                join a in db.Accounts on l.AccountId equals a.Id
                join t in db.ThirdParties on l.ThirdPartyId equals t.Id into t0
                from t in t0.DefaultIfEmpty()
                where e.CompanyId == DefaultCompanyId && e.Status == "POSTED" && e.Date >= fromDate && e.Date <= toDate
                select new { e.Date, e.Number, e.Type, a.Code, a.Name, l.Description, l.Category, ThirdName = (string?)t.RazonSocial, ThirdId = (long?)l.ThirdPartyId, l.Debit, l.Credit, HeaderThirdId = (long?)e.ThirdPartyId };
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(x => x.Type == type);
        if (thirdPartyId is not null) q = q.Where(x => x.ThirdId == thirdPartyId || x.HeaderThirdId == thirdPartyId);
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(x => x.Category == category);
        var rows = await q.OrderBy(x => x.Date).ThenBy(x => x.Number).ThenBy(x => x.Code)
            .Select(x => new JournalRowDto(x.Date, x.Number, x.Type, x.Code, x.Name, x.Description, x.Category, x.ThirdName, x.Debit, x.Credit))
            .ToListAsync(ct);
        return rows;
    }

    public async Task<IReadOnlyList<LedgerRowDto>> GetLedgerAsync(long accountId, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.CompanyId == DefaultCompanyId, ct) ?? throw new InvalidOperationException("Cuenta no encontrada");
        var rows = await (from l in db.JournalLines
                          join e in db.JournalEntries on l.JournalEntryId equals e.Id
                          join a in db.Accounts on l.AccountId equals a.Id
                          join t in db.ThirdParties on l.ThirdPartyId equals t.Id into t0
                          from t in t0.DefaultIfEmpty()
                          where e.CompanyId == DefaultCompanyId && e.Status == "POSTED" && e.Date >= fromDate && e.Date <= toDate && l.AccountId == accountId
                          orderby e.Date, e.Number, l.Id
                          select new { e.Date, e.Number, e.Type, a.Code, a.Name, l.Description, l.Category, ThirdName = (string?)t.RazonSocial, l.Debit, l.Credit })
                          .ToListAsync(ct);
        decimal running = 0;
        var nature = account.Nature;
        var list = new List<LedgerRowDto>();
        foreach (var r in rows)
        {
            running += (nature == 'D') ? (r.Debit - r.Credit) : (r.Credit - r.Debit);
            list.Add(new LedgerRowDto(r.Date, r.Number, r.Type, r.Code, r.Name, r.Description, r.Category, r.ThirdName, r.Debit, r.Credit, Round(running)));
        }
        return list;
    }
}
