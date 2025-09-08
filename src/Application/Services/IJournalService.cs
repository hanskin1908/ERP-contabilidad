using Application.Dtos;

namespace Application.Services;

public interface IJournalService
{
    Task<IReadOnlyList<JournalEntrySummaryDto>> SearchAsync(DateOnly? from, DateOnly? to, string? type, long? thirdPartyId, string? q, CancellationToken ct);
    Task<JournalEntryDto?> GetAsync(long id, CancellationToken ct);
    Task<JournalEntryDto> CreateDraftAsync(CreateJournalEntryRequest req, CancellationToken ct);
    Task<JournalEntryDto?> UpdateDraftAsync(long id, UpdateJournalEntryRequest req, CancellationToken ct);
    Task<bool> PostAsync(long id, CancellationToken ct);
    Task<bool> VoidAsync(long id, CancellationToken ct);
}

