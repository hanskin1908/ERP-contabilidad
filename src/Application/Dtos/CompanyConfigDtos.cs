namespace Application.Dtos;

public record CompanyConfigDto(long CompanyId, long? CxcAccountId, long? CxpAccountId, long? IvaVentasAccountId, long? IvaComprasAccountId);

public class UpdateCompanyConfigRequest
{
    public long? CxcAccountId { get; set; }
    public long? CxpAccountId { get; set; }
    public long? IvaVentasAccountId { get; set; }
    public long? IvaComprasAccountId { get; set; }
}

