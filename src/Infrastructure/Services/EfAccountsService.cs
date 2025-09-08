using Application.Dtos;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class EfAccountsService(ApplicationDbContext db) : IAccountsService
{
    private const long DefaultCompanyId = 1;

    public async Task<IReadOnlyList<AccountDto>> SearchAsync(string? search, bool? onlyPostable, bool? active, CancellationToken ct)
    {
        var q = db.Accounts.AsNoTracking().Where(a => a.CompanyId == DefaultCompanyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(a => a.Code.Contains(s) || a.Name.Contains(s));
        }
        if (onlyPostable is not null)
            q = q.Where(a => a.IsPostable == onlyPostable);
        if (active is not null)
            q = q.Where(a => a.IsActive == active);

        var list = await q.OrderBy(a => a.Code)
            .Select(a => new AccountDto(a.Id, a.Code, a.Name, a.Level, a.Nature, a.ParentId, a.IsPostable, a.IsActive))
            .ToListAsync(ct);
        return list;
    }

    public async Task<AccountDto?> GetAsync(long id, CancellationToken ct)
    {
        var a = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        return a is null ? null : new AccountDto(a.Id, a.Code, a.Name, a.Level, a.Nature, a.ParentId, a.IsPostable, a.IsActive);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest req, CancellationToken ct)
    {
        var entity = new Account
        {
            CompanyId = DefaultCompanyId,
            Code = req.Code.Trim(),
            Name = req.Name.Trim(),
            Level = req.Level,
            Nature = req.Nature,
            ParentId = req.ParentId,
            IsPostable = req.IsPostable,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Accounts.Add(entity);
        await db.SaveChangesAsync(ct);
        return new AccountDto(entity.Id, entity.Code, entity.Name, entity.Level, entity.Nature, entity.ParentId, entity.IsPostable, entity.IsActive);
    }

    public async Task<AccountDto?> UpdateAsync(long id, UpdateAccountRequest req, CancellationToken ct)
    {
        var a = await db.Accounts.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (a is null) return null;
        a.Name = req.Name.Trim();
        a.Level = req.Level;
        a.Nature = req.Nature;
        a.ParentId = req.ParentId;
        a.IsPostable = req.IsPostable;
        a.IsActive = req.IsActive;
        a.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return new AccountDto(a.Id, a.Code, a.Name, a.Level, a.Nature, a.ParentId, a.IsPostable, a.IsActive);
    }

    public async Task<bool> SetActiveAsync(long id, bool active, CancellationToken ct)
    {
        var a = await db.Accounts.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == DefaultCompanyId, ct);
        if (a is null) return false;
        a.IsActive = active;
        a.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }
}

