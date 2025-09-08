using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize(Roles = "admin,operador")]
[Route("api/journal-entries")]
public class JournalEntriesController(IJournalService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JournalEntrySummaryDto>>> Search([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? type, [FromQuery] long? thirdPartyId, [FromQuery] string? q, CancellationToken ct)
    {
        var data = await service.SearchAsync(from, to, type, thirdPartyId, q, ct);
        return Ok(data);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<JournalEntryDto>> Get(long id, CancellationToken ct)
    {
        var j = await service.GetAsync(id, ct);
        return j is null ? NotFound() : Ok(j);
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntryDto>> Create([FromBody] CreateJournalEntryRequest req, CancellationToken ct)
    {
        try
        {
            var j = await service.CreateDraftAsync(req, ct);
            return CreatedAtAction(nameof(Get), new { id = j.Id }, j);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<JournalEntryDto>> Update(long id, [FromBody] UpdateJournalEntryRequest req, CancellationToken ct)
    {
        try
        {
            var j = await service.UpdateDraftAsync(id, req, ct);
            return j is null ? NotFound() : Ok(j);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPost("{id:long}/post")]
    [Authorize(Roles = "admin")] // publicar asientos solo admin
    public async Task<ActionResult> Post(long id, CancellationToken ct)
    {
        try
        {
            var ok = await service.PostAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPost("{id:long}/void")]
    public async Task<ActionResult> Void(long id, CancellationToken ct)
    {
        var ok = await service.VoidAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}
