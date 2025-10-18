using InvoicesService.Application.DTOs.Requests;
using InvoicesService.Application.DTOs.Responses;

namespace InvoicesService.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<InvoiceResponse> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<InvoiceListResponse>> GetInvoicesAsync(
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<InvoiceResponse> UpdateInvoiceAsync(Guid id, UpdateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task DeleteInvoiceAsync(Guid id, CancellationToken cancellationToken = default);
}
