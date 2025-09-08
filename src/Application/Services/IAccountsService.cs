using Application.Dtos;

namespace Application.Services;

public interface IAccountsService
{
    Task<IReadOnlyList<AccountDto>> SearchAsync(string? search, bool? onlyPostable, bool? active, CancellationToken ct);
    Task<AccountDto?> GetAsync(long id, CancellationToken ct);
    Task<AccountDto> CreateAsync(CreateAccountRequest req, CancellationToken ct);
    Task<AccountDto?> UpdateAsync(long id, UpdateAccountRequest req, CancellationToken ct);
    Task<bool> SetActiveAsync(long id, bool active, CancellationToken ct);
}

