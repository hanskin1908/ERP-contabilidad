using Application.Dtos;

namespace Application.Services;

public interface IInvoicesService
{
    Task<IReadOnlyList<InvoiceDto>> SearchAsync(string? type, string? status, DateOnly? from, DateOnly? to, CancellationToken ct);
    Task<InvoiceDto?> GetAsync(long id, CancellationToken ct);
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest req, CancellationToken ct);
    Task<InvoiceDto?> UpdateAsync(long id, UpdateInvoiceRequest req, CancellationToken ct);
    Task<bool> ApproveAsync(long id, ApproveInvoiceRequest req, CancellationToken ct);
    Task<bool> CancelAsync(long id, CancellationToken ct);
}

