namespace Application.Dtos;

public record JournalLineDto(
    long Id,
    long AccountId,
    string? Description,
    string? Category,
    decimal Debit,
    decimal Credit,
    long? ThirdPartyId
);

public record JournalEntrySummaryDto(
    long Id,
    long Number,
    string Type,
    DateOnly Date,
    string Status,
    decimal TotalDebit,
    decimal TotalCredit
);

public record JournalEntryDto(
    long Id,
    long Number,
    string Type,
    DateOnly Date,
    string? Description,
    string Status,
    decimal TotalDebit,
    decimal TotalCredit,
    List<JournalLineDto> Lines
);

public class CreateJournalLineRequest
{
    public long AccountId { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public long? ThirdPartyId { get; set; }
}

public class CreateJournalEntryRequest
{
    public string Type { get; set; } = "DIARIO"; // INGRESO|EGRESO|AJUSTE|DIARIO
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public long? ThirdPartyId { get; set; }
    public List<CreateJournalLineRequest> Lines { get; set; } = new();
}

public class UpdateJournalEntryRequest
{
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public long? ThirdPartyId { get; set; }
    public List<CreateJournalLineRequest> Lines { get; set; } = new();
}
