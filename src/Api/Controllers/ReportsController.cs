using System.Text;
using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController(IReportsService reports) : ControllerBase
{
    [HttpGet("income-statement")]
    public async Task<IActionResult> IncomeStatement([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? format, CancellationToken ct)
    {
        var dto = await reports.GetIncomeStatementAsync(from, to, ct);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = new StringBuilder();
            csv.AppendLine("Desde,Hasta,Ingresos,Costos,Gastos,Utilidad");
            csv.AppendLine($"{dto.From:yyyy-MM-dd},{dto.To:yyyy-MM-dd},{dto.Ingresos},{dto.Costos},{dto.Gastos},{dto.Utilidad}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"income-statement_{dto.From:yyyyMMdd}_{dto.To:yyyyMMdd}.csv");
        }
        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = GeneratePdfTable($"Estado de Resultados {from:yyyy-MM-dd} a {to:yyyy-MM-dd}", new[] { "Concepto", "Valor" }, new List<string[]>
            {
                new [] { "Ingresos", dto.Ingresos.ToString("N2") },
                new [] { "Costos", dto.Costos.ToString("N2") },
                new [] { "Gastos", dto.Gastos.ToString("N2") },
                new [] { "Utilidad", dto.Utilidad.ToString("N2") }
            });
            return File(bytes, "application/pdf", $"income-statement_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }
        return Ok(dto);
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> BalanceSheet([FromQuery] DateOnly asOf, [FromQuery] string? format, CancellationToken ct)
    {
        var dto = await reports.GetBalanceSheetAsync(asOf, ct);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = new StringBuilder();
            csv.AppendLine("Fecha,Activos,Pasivos,Patrimonio");
            csv.AppendLine($"{dto.AsOf:yyyy-MM-dd},{dto.Activos},{dto.Pasivos},{dto.Patrimonio}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"balance-sheet_{dto.AsOf:yyyyMMdd}.csv");
        }
        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = GeneratePdfTable($"Balance General {dto.AsOf:yyyy-MM-dd}", new[] { "Concepto", "Valor" }, new List<string[]>
            {
                new [] { "Activos", dto.Activos.ToString("N2") },
                new [] { "Pasivos", dto.Pasivos.ToString("N2") },
                new [] { "Patrimonio", dto.Patrimonio.ToString("N2") }
            });
            return File(bytes, "application/pdf", $"balance-sheet_{dto.AsOf:yyyyMMdd}.pdf");
        }
        return Ok(dto);
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> TrialBalance([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? format, CancellationToken ct)
    {
        var rows = await reports.GetTrialBalanceAsync(from, to, ct);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = new StringBuilder();
            csv.AppendLine("Cuenta,Nombre,Débitos,Créditos,Saldo");
            foreach (var r in rows)
                csv.AppendLine($"{r.AccountCode},{r.AccountName},{r.Debits},{r.Credits},{r.Balance}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"trial-balance_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
        }
        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = GeneratePdfTable($"Balance de Comprobación {from:yyyy-MM-dd} a {to:yyyy-MM-dd}", new[] { "Cuenta", "Nombre", "Débitos", "Créditos", "Saldo" }, rows.Select(r => new[] { r.AccountCode, r.AccountName, r.Debits.ToString("N2"), r.Credits.ToString("N2"), r.Balance.ToString("N2") }).ToList());
            return File(bytes, "application/pdf", $"trial-balance_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }
        return Ok(rows);
    }

    [HttpGet("journal")]
    public async Task<IActionResult> Journal([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? type, [FromQuery] long? thirdPartyId, [FromQuery] string? category, [FromQuery] string? format, CancellationToken ct)
    {
        var rows = await reports.GetJournalAsync(from, to, type, thirdPartyId, category, ct);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = new StringBuilder();
            csv.AppendLine("Fecha,Número,Tipo,Cuenta,Nombre,Detalle,Categoría,Tercero,Débito,Crédito");
            foreach (var r in rows)
                csv.AppendLine($"{r.Date:yyyy-MM-dd},{r.Number},{r.Type},{r.AccountCode},{r.AccountName},\"{r.Description}\",{r.Category},{r.ThirdName},{r.Debit},{r.Credit}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"journal_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
        }
        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = GeneratePdfTable($"Libro Diario {from:yyyy-MM-dd} a {to:yyyy-MM-dd}", new[] { "Fecha", "#", "Tipo", "Cuenta", "Nombre", "Detalle", "Cat.", "Tercero", "Débito", "Crédito" }, rows.Select(r => new[] { r.Date.ToString("yyyy-MM-dd"), r.Number.ToString(), r.Type, r.AccountCode, r.AccountName, r.Description ?? "", r.Category ?? "", r.ThirdName ?? "", r.Debit.ToString("N2"), r.Credit.ToString("N2") }).ToList());
            return File(bytes, "application/pdf", $"journal_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }
        return Ok(rows);
    }

    [HttpGet("ledger")]
    public async Task<IActionResult> Ledger([FromQuery] long accountId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string? format, CancellationToken ct)
    {
        var rows = await reports.GetLedgerAsync(accountId, from, to, ct);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = new StringBuilder();
            csv.AppendLine("Fecha,Número,Tipo,Cuenta,Nombre,Detalle,Categoría,Tercero,Débito,Crédito,Saldo");
            foreach (var r in rows)
                csv.AppendLine($"{r.Date:yyyy-MM-dd},{r.Number},{r.Type},{r.AccountCode},{r.AccountName},\"{r.Description}\",{r.Category},{r.ThirdName},{r.Debit},{r.Credit},{r.RunningBalance}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"ledger_{accountId}_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
        }
        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = GeneratePdfTable($"Libro Mayor {from:yyyy-MM-dd} a {to:yyyy-MM-dd}", new[] { "Fecha", "#", "Tipo", "Cuenta", "Nombre", "Detalle", "Cat.", "Tercero", "Débito", "Crédito", "Saldo" }, rows.Select(r => new[] { r.Date.ToString("yyyy-MM-dd"), r.Number.ToString(), r.Type, r.AccountCode, r.AccountName, r.Description ?? "", r.Category ?? "", r.ThirdName ?? "", r.Debit.ToString("N2"), r.Credit.ToString("N2"), r.RunningBalance.ToString("N2") }).ToList());
            return File(bytes, "application/pdf", $"ledger_{accountId}_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }
        return Ok(rows);
    }

    private static byte[] GeneratePdfTable(string title, string[] headers, List<string[]> rows)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var ms = new MemoryStream();
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text(title).SemiBold().FontSize(16);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns => { foreach (var _ in headers) columns.RelativeColumn(); });
                    table.Header(header =>
                    {
                        foreach (var h in headers)
                            header.Cell().Element(CellStyle).Text(h).SemiBold();
                    });
                    foreach (var r in rows)
                    {
                        foreach (var c in r)
                            table.Cell().Element(CellStyle).Text(c);
                    }
                    IContainer CellStyle(IContainer c) => c.Padding(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                });
                page.Footer().AlignRight().Text(txt => txt.Span("Generado por ERP").FontSize(10));
            });
        }).GeneratePdf(ms);
        return ms.ToArray();
    }
}
