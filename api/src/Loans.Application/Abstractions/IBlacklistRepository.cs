using Loans.Domain;

namespace Loans.Application.Abstractions;

public interface IBlacklistRepository
{
    Task<IReadOnlyList<BlacklistedSsn>> ListAsync(CancellationToken cancellationToken);
    Task<BlacklistedSsn?> FindAsync(Ssn ssn, CancellationToken cancellationToken);
    void Add(BlacklistedSsn entry);
    void Remove(BlacklistedSsn entry);
}
