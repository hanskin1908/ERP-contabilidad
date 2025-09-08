namespace Domain.Entities;

public class Attachment
{
    public long Id { get; set; }
    public string EntityType { get; set; } = string.Empty; // journal_entry, invoice, etc.
    public long EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

