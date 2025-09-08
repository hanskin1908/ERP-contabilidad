using Application.Dtos;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class EfCompanyConfigService(ApplicationDbContext db) : ICompanyConfigService
{
    private const long DefaultCompanyId = 1;

    public async Task<CompanyConfigDto> GetAsync(CancellationToken ct)
    {
        var cfg = await db.CompanyConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyId == DefaultCompanyId, ct);
        if (cfg is null)
        {
            cfg = new CompanyConfig { CompanyId = DefaultCompanyId };
            db.CompanyConfigs.Add(cfg);
            await db.SaveChangesAsync(ct);
        }
        return new CompanyConfigDto(cfg.CompanyId, cfg.CxcAccountId, cfg.CxpAccountId, cfg.IvaVentasAccountId, cfg.IvaComprasAccountId);
    }

    public async Task<CompanyConfigDto> UpdateAsync(UpdateCompanyConfigRequest req, CancellationToken ct)
    {
        var cfg = await db.CompanyConfigs.FirstOrDefaultAsync(c => c.CompanyId == DefaultCompanyId, ct);
        if (cfg is null)
        {
            cfg = new CompanyConfig { CompanyId = DefaultCompanyId };
            db.CompanyConfigs.Add(cfg);
        }
        cfg.CxcAccountId = req.CxcAccountId;
        cfg.CxpAccountId = req.CxpAccountId;
        cfg.IvaVentasAccountId = req.IvaVentasAccountId;
        cfg.IvaComprasAccountId = req.IvaComprasAccountId;
        cfg.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return await GetAsync(ct);
    }
}

