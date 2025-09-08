using Application.Dtos;

namespace Application.Services;

public interface IReportsService
{
    Task<IncomeStatementDto> GetIncomeStatementAsync(DateOnly from, DateOnly to, CancellationToken ct);
    Task<BalanceSheetDto> GetBalanceSheetAsync(DateOnly asOf, CancellationToken ct);
    Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalanceAsync(DateOnly fromDate, DateOnly toDate, CancellationToken ct);
    Task<IReadOnlyList<JournalRowDto>> GetJournalAsync(DateOnly fromDate, DateOnly toDate, string? type, long? thirdPartyId, string? category, CancellationToken ct);
    Task<IReadOnlyList<LedgerRowDto>> GetLedgerAsync(long accountId, DateOnly fromDate, DateOnly toDate, CancellationToken ct);
}
