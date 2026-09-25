using Loans.Application.Abstractions;
using Loans.Domain;

namespace Loans.Application.Configuration;

public sealed class BlacklistService(IBlacklistRepository blacklist, IUnitOfWork unitOfWork, TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<Ssn>> ListAsync(CancellationToken cancellationToken) =>
        (await blacklist.ListAsync(cancellationToken)).Select(entry => entry.Ssn).ToList();

    /// <returns><c>false</c> when the SSN is already blacklisted.</returns>
    public async Task<bool> AddAsync(Ssn ssn, CancellationToken cancellationToken)
    {
        if (await blacklist.FindAsync(ssn, cancellationToken) is not null)
        {
            return false;
        }

        blacklist.Add(new BlacklistedSsn(ssn, timeProvider.GetUtcNow().UtcDateTime));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <returns><c>false</c> when the SSN is not blacklisted.</returns>
    public async Task<bool> RemoveAsync(Ssn ssn, CancellationToken cancellationToken)
    {
        var entry = await blacklist.FindAsync(ssn, cancellationToken);
        if (entry is null)
        {
            return false;
        }

        blacklist.Remove(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
