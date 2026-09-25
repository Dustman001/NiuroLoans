using Loans.Application.Abstractions;
using Loans.Domain;
using Microsoft.EntityFrameworkCore;

namespace Loans.Infrastructure.Persistence.Repositories;

public sealed class StateRepository(LoansDbContext db) : IStateRepository
{
    public async Task<IReadOnlyList<State>> ListAsync(CancellationToken cancellationToken) =>
        await db.States.OrderBy(s => s.Name).ToListAsync(cancellationToken);

    public Task<State?> FindAsync(int stateId, CancellationToken cancellationToken) =>
        db.States.SingleOrDefaultAsync(s => s.Id == stateId, cancellationToken);
}
