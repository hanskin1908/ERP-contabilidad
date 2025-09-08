namespace Domain.Entities;

public class CompanyConfig
{
    public long CompanyId { get; set; }
    public long? CxcAccountId { get; set; }
    public long? CxpAccountId { get; set; }
    public long? IvaVentasAccountId { get; set; }
    public long? IvaComprasAccountId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

