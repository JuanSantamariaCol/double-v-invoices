using InvoicesService.Domain.Entities;

namespace InvoicesService.Domain.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<(List<Invoice> Invoices, int TotalCount)> GetAllAsync(
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default);
}
