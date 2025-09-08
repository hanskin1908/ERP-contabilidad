namespace Domain.Entities;

public class JournalEntry
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public long Number { get; set; }
    public string Type { get; set; } = "DIARIO"; // INGRESO|EGRESO|AJUSTE|DIARIO
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public long? ThirdPartyId { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string Status { get; set; } = "DRAFT"; // DRAFT|POSTED|VOID
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

