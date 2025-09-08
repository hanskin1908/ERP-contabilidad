using Application.Dtos;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class EfThirdPartiesService(ApplicationDbContext db) : IThirdPartiesService
{
    private const long DefaultCompanyId = 1;

    public async Task<IReadOnlyList<ThirdPartyDto>> SearchAsync(string? search, string? type, bool? active, CancellationToken ct)
    {
        var q = db.ThirdParties.AsNoTracking().Where(t => t.CompanyId == DefaultCompanyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(t => t.Nit.Contains(s) || t.RazonSocial.Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(type))
            q = q.Where(t => t.Tipo == type);
        if (active is not null)
            q = q.Where(t => t.IsActive == active);

        var list = await q.OrderBy(t => t.RazonSocial)
            .Select(t => new ThirdPartyDto(t.Id, t.Nit, t.Dv, t.Tipo, t.RazonSocial, t.Direccion, t.Email, t.Telefono, t.IsActive))
            .ToListAsync(ct);
        return list;
    }

    public async Task<ThirdPartyDto?> GetAsync(long id, CancellationToken ct)
    {
        var t = await db.ThirdParties.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        return t is null ? null : new ThirdPartyDto(t.Id, t.Nit, t.Dv, t.Tipo, t.RazonSocial, t.Direccion, t.Email, t.Telefono, t.IsActive);
    }

    public async Task<ThirdPartyDto> CreateAsync(CreateThirdPartyRequest req, CancellationToken ct)
    {
        var entity = new ThirdParty
        {
            CompanyId = DefaultCompanyId,
            Nit = req.Nit.Trim(),
            Dv = req.Dv?.Trim(),
            Tipo = req.Tipo,
            RazonSocial = req.RazonSocial.Trim(),
            Direccion = req.Direccion?.Trim(),
            Email = req.Email?.Trim(),
            Telefono = req.Telefono?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.ThirdParties.Add(entity);
        await db.SaveChangesAsync(ct);
        return new ThirdPartyDto(entity.Id, entity.Nit, entity.Dv, entity.Tipo, entity.RazonSocial, entity.Direccion, entity.Email, entity.Telefono, entity.IsActive);
    }

    public async Task<ThirdPartyDto?> UpdateAsync(long id, UpdateThirdPartyRequest req, CancellationToken ct)
    {
        var t = await db.ThirdParties.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (t is null) return null;
        t.Tipo = req.Tipo;
        t.RazonSocial = req.RazonSocial.Trim();
        t.Direccion = req.Direccion?.Trim();
        t.Email = req.Email?.Trim();
        t.Telefono = req.Telefono?.Trim();
        t.IsActive = req.IsActive;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return new ThirdPartyDto(t.Id, t.Nit, t.Dv, t.Tipo, t.RazonSocial, t.Direccion, t.Email, t.Telefono, t.IsActive);
    }

    public async Task<bool> SetActiveAsync(long id, bool active, CancellationToken ct)
    {
        var t = await db.ThirdParties.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (t is null) return false;
        t.IsActive = active;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }
}

