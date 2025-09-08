using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "admin,operador")]
[Route("api/third-parties")]
public class ThirdPartiesController(IThirdPartiesService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ThirdPartyDto>>> Search([FromQuery] string? search, [FromQuery] string? type, [FromQuery] bool? active, CancellationToken ct)
    {
        var data = await service.SearchAsync(search, type, active, ct);
        return Ok(data);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ThirdPartyDto>> Get(long id, CancellationToken ct)
    {
        var t = await service.GetAsync(id, ct);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost]
    public async Task<ActionResult<ThirdPartyDto>> Create([FromBody] CreateThirdPartyRequest req, CancellationToken ct)
    {
        try
        {
            var t = await service.CreateAsync(req, ct);
            return CreatedAtAction(nameof(Get), new { id = t.Id }, t);
        }
        catch (Exception ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ThirdPartyDto>> Update(long id, [FromBody] UpdateThirdPartyRequest req, CancellationToken ct)
    {
        var t = await service.UpdateAsync(id, req, ct);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPatch("{id:long}/activate")]
    [Authorize(Roles = "admin")] // solo admin activa/inactiva
    public async Task<ActionResult> Activate(long id, CancellationToken ct)
    {
        var ok = await service.SetActiveAsync(id, true, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("{id:long}/deactivate")]
    [Authorize(Roles = "admin")] // solo admin activa/inactiva
    public async Task<ActionResult> Deactivate(long id, CancellationToken ct)
    {
        var ok = await service.SetActiveAsync(id, false, ct);
        return ok ? NoContent() : NotFound();
    }
}
