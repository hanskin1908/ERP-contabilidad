namespace Domain.Entities;

public class Invoice
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string Type { get; set; } = "SALE"; // SALE|PURCHASE
    public long Number { get; set; }
    public long ThirdPartyId { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Currency { get; set; } = "COP";
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "DRAFT"; // DRAFT|APPROVED|CANCELLED
    public long? JournalEntryId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

