using InvoicesService.Domain.Entities;

namespace InvoicesService.Domain.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsPublishedAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid id, string errorMessage, CancellationToken cancellationToken = default);
}
