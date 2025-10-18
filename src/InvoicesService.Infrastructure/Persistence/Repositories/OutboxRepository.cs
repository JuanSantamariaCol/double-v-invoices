using InvoicesService.Domain.Entities;
using InvoicesService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoicesService.Infrastructure.Persistence.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly InvoicesDbContext _context;

    public OutboxRepository(InvoicesDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _context.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .Where(m => m.Status == "pending")
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsPublishedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message != null)
        {
            message.MarkAsPublished();
            _context.OutboxMessages.Update(message);
        }
    }

    public async Task MarkAsFailedAsync(Guid id, string errorMessage, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message != null)
        {
            message.MarkAsFailed(errorMessage);
            _context.OutboxMessages.Update(message);
        }
    }
}
