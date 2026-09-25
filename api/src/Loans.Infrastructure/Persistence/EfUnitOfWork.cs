using Loans.Application.Abstractions;

namespace Loans.Infrastructure.Persistence;

public sealed class EfUnitOfWork(LoansDbContext db) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await work(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            // Disposing an uncommitted transaction rolls it back. Clearing the tracker drops the
            // in-memory changes too, so a retry starts from what is really in the database.
            db.ChangeTracker.Clear();
            throw;
        }
    }
}
