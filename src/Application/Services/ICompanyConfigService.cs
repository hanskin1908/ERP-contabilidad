using Application.Dtos;

namespace Application.Services;

public interface ICompanyConfigService
{
    Task<CompanyConfigDto> GetAsync(CancellationToken ct);
    Task<CompanyConfigDto> UpdateAsync(UpdateCompanyConfigRequest req, CancellationToken ct);
}

