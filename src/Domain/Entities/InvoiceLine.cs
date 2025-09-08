namespace Domain.Entities;

public class InvoiceLine
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public long AccountId { get; set; }
    public decimal Total { get; set; }
}

