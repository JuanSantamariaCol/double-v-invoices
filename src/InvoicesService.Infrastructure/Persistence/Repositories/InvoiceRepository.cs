using InvoicesService.Domain.Entities;
using InvoicesService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoicesService.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoicesDbContext _context;

    public InvoiceRepository(InvoicesDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<(List<Invoice> Invoices, int TotalCount)> GetAllAsync(
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Invoices
            .Include(i => i.Items)
            .Where(i => i.IsActive)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(i => i.IssueDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(i => i.IssueDate <= endDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var invoices = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (invoices, totalCount);
    }

    public async Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
        return invoice;
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Update(invoice);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await GetByIdAsync(id, cancellationToken);
        if (invoice != null)
        {
            invoice.SoftDelete();
            _context.Invoices.Update(invoice);
        }
    }

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var lastInvoice = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int sequenceNumber = 1;

        if (lastInvoice != null)
        {
            var lastNumber = lastInvoice.InvoiceNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out int lastSequence))
            {
                sequenceNumber = lastSequence + 1;
            }
        }

        return $"{prefix}{sequenceNumber:D6}";
    }
}
