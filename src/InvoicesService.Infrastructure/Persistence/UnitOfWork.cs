using InvoicesService.Domain.Interfaces;

namespace InvoicesService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly InvoicesDbContext _context;

    public UnitOfWork(InvoicesDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.RollbackTransactionAsync(cancellationToken);
    }
}
