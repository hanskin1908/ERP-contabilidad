namespace Application.Dtos;

public record InvoiceLineDto(
    long Id,
    string ItemName,
    decimal Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal TaxRate,
    long AccountId,
    decimal Total
);

public record InvoiceDto(
    long Id,
    string Type,
    long Number,
    long ThirdPartyId,
    DateOnly IssueDate,
    DateOnly? DueDate,
    string Currency,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    string Status,
    long? JournalEntryId,
    string? Notes,
    List<InvoiceLineDto> Lines
);

public class CreateInvoiceLineRequest
{
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public long AccountId { get; set; }
}

public class CreateInvoiceRequest
{
    public string Type { get; set; } = "SALE"; // SALE|PURCHASE
    public long ThirdPartyId { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Currency { get; set; } = "COP";
    public List<CreateInvoiceLineRequest> Lines { get; set; } = new();
    public string? Notes { get; set; }
}

public class UpdateInvoiceRequest
{
    public DateOnly IssueDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Currency { get; set; } = "COP";
    public List<CreateInvoiceLineRequest> Lines { get; set; } = new();
    public string? Notes { get; set; }
}

public class ApproveInvoiceRequest
{
    public DateOnly? PostingDate { get; set; }
    public long CounterAccountId { get; set; } // CxC (SALE) o CxP (PURCHASE)
}

