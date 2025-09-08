using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoicesController(IInvoicesService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> Search([FromQuery] string? type, [FromQuery] string? status, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var data = await service.SearchAsync(type, status, from, to, ct);
        return Ok(data);
    }

    [HttpGet("{id:long}/pdf")]
    public async Task<IActionResult> Pdf([FromServices] Infrastructure.Persistence.ApplicationDbContext db, long id, CancellationToken ct)
    {
        var inv = await db.Invoices.Include(i => i.CompanyId).FirstOrDefaultAsync(i => i.Id == id, ct);
        if (inv is null) return NotFound();
        var lines = await db.InvoiceLines.Where(l => l.InvoiceId == inv.Id).ToListAsync(ct);
        var third = await db.ThirdParties.FirstOrDefaultAsync(t => t.Id == inv.ThirdPartyId, ct);
        QuestPDF.Settings.License = LicenseType.Community;
        var ms = new MemoryStream();
        Document.Create(c =>
        {
            c.Page(p =>
            {
                p.Margin(20);
                p.Header().Row(r =>
                {
                    r.RelativeItem().Text($"Factura {inv.Type}-{inv.Number}").SemiBold().FontSize(16);
                    r.ConstantItem(200).AlignRight().Text($"Fecha: {inv.IssueDate:yyyy-MM-dd}");
                });
                p.Content().Column(col =>
                {
                    col.Item().Text($"Tercero: {third?.RazonSocial} NIT: {third?.Nit}");
                    col.Item().LineHorizontal(0.5f);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cn => { cn.RelativeColumn(4); cn.RelativeColumn(); cn.RelativeColumn(); cn.RelativeColumn(); cn.RelativeColumn(); cn.RelativeColumn(3); });
                        t.Header(h =>
                        {
                            h.Cell().Text("Ítem");
                            h.Cell().Text("Cant.");
                            h.Cell().Text("V.Unit");
                            h.Cell().Text("Desc.");
                            h.Cell().Text("IVA %");
                            h.Cell().Text("Total");
                        });
                        foreach (var l in lines)
                        {
                            var baseV = (l.Quantity * l.UnitPrice) - l.Discount;
                            var tax = Math.Round((double)(baseV * (l.TaxRate / 100m)), 2);
                            var total = Math.Round((double)(baseV + (decimal)tax), 2);
                            t.Cell().Text(l.ItemName);
                            t.Cell().Text(l.Quantity.ToString("N2"));
                            t.Cell().Text(l.UnitPrice.ToString("N2"));
                            t.Cell().Text(l.Discount.ToString("N2"));
                            t.Cell().Text(l.TaxRate.ToString("N2"));
                            t.Cell().Text(total.ToString("N2"));
                        }
                    });
                    col.Item().AlignRight().Text($"Subtotal: {inv.Subtotal:N2}");
                    col.Item().AlignRight().Text($"Impuestos: {inv.TaxTotal:N2}");
                    col.Item().AlignRight().Text($"Total: {inv.Total:N2}");
                });
                p.Footer().AlignRight().Text("Generado por ERP").FontSize(10);
            });
        }).GeneratePdf(ms);
        return File(ms.ToArray(), "application/pdf", $"invoice_{inv.Type}_{inv.Number}.pdf");
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<InvoiceDto>> Get(long id, CancellationToken ct)
    {
        var d = await service.GetAsync(id, ct);
        return d is null ? NotFound() : Ok(d);
    }

    [HttpPost]
    [Authorize(Roles = "admin,operador")]
    public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceRequest req, CancellationToken ct)
    {
        try
        {
            var d = await service.CreateAsync(req, ct);
            return CreatedAtAction(nameof(Get), new { id = d.Id }, d);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "admin,operador")]
    public async Task<ActionResult<InvoiceDto>> Update(long id, [FromBody] UpdateInvoiceRequest req, CancellationToken ct)
    {
        try
        {
            var d = await service.UpdateAsync(id, req, ct);
            return d is null ? NotFound() : Ok(d);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult> Approve(long id, [FromBody] ApproveInvoiceRequest req, CancellationToken ct)
    {
        try
        {
            var ok = await service.ApproveAsync(id, req, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    [HttpPost("{id:long}/cancel")]
    [Authorize(Roles = "admin,operador")]
    public async Task<ActionResult> Cancel(long id, CancellationToken ct)
    {
        try
        {
            var ok = await service.CancelAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }
}
