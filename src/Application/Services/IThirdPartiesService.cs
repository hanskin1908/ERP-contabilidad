using Application.Dtos;

namespace Application.Services;

public interface IThirdPartiesService
{
    Task<IReadOnlyList<ThirdPartyDto>> SearchAsync(string? search, string? type, bool? active, CancellationToken ct);
    Task<ThirdPartyDto?> GetAsync(long id, CancellationToken ct);
    Task<ThirdPartyDto> CreateAsync(CreateThirdPartyRequest req, CancellationToken ct);
    Task<ThirdPartyDto?> UpdateAsync(long id, UpdateThirdPartyRequest req, CancellationToken ct);
    Task<bool> SetActiveAsync(long id, bool active, CancellationToken ct);
}

