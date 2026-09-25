using Loans.Domain;

namespace Loans.Application.Abstractions;

public interface IStateRepository
{
    Task<IReadOnlyList<State>> ListAsync(CancellationToken cancellationToken);
    Task<State?> FindAsync(int stateId, CancellationToken cancellationToken);
}
