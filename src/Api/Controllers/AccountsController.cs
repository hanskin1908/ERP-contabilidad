using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "admin,operador")]
[Route("api/accounts")]
public class AccountsController(IAccountsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> Search([FromQuery] string? search, [FromQuery] bool? onlyPostable, [FromQuery] bool? active, CancellationToken ct)
    {
        var data = await service.SearchAsync(search, onlyPostable, active, ct);
        return Ok(data);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AccountDto>> Get(long id, CancellationToken ct)
    {
        var a = await service.GetAsync(id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost]
    [Authorize(Roles = "admin")] // solo admin crea
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountRequest req, CancellationToken ct)
    {
        try
        {
            var a = await service.CreateAsync(req, ct);
            return CreatedAtAction(nameof(Get), new { id = a.Id }, a);
        }
        catch (Exception ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "admin")] // solo admin edita
    public async Task<ActionResult<AccountDto>> Update(long id, [FromBody] UpdateAccountRequest req, CancellationToken ct)
    {
        var a = await service.UpdateAsync(id, req, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPatch("{id:long}/activate")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Activate(long id, CancellationToken ct)
    {
        var ok = await service.SetActiveAsync(id, true, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPatch("{id:long}/deactivate")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Deactivate(long id, CancellationToken ct)
    {
        var ok = await service.SetActiveAsync(id, false, ct);
        return ok ? NoContent() : NotFound();
    }
}
