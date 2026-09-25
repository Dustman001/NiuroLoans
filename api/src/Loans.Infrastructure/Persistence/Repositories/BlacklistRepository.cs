using Loans.Application.Abstractions;
using Loans.Domain;
using Microsoft.EntityFrameworkCore;

namespace Loans.Infrastructure.Persistence.Repositories;

public sealed class BlacklistRepository(LoansDbContext db) : IBlacklistRepository
{
    public async Task<IReadOnlyList<BlacklistedSsn>> ListAsync(CancellationToken cancellationToken) =>
        await db.Blacklist.OrderBy(b => b.Ssn).ToListAsync(cancellationToken);

    public Task<BlacklistedSsn?> FindAsync(Ssn ssn, CancellationToken cancellationToken) =>
        db.Blacklist.SingleOrDefaultAsync(b => b.Ssn == ssn, cancellationToken);

    public void Add(BlacklistedSsn entry) => db.Blacklist.Add(entry);

    public void Remove(BlacklistedSsn entry) => db.Blacklist.Remove(entry);
}
