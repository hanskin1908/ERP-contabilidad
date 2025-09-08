namespace Domain.Entities;

public class JournalLine
{
    public long Id { get; set; }
    public long JournalEntryId { get; set; }
    public long AccountId { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public long? ThirdPartyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
