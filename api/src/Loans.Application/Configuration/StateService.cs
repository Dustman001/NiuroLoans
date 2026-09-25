using Loans.Application.Abstractions;
using Loans.Domain;

namespace Loans.Application.Configuration;

public sealed class StateService(IStateRepository states, IUnitOfWork unitOfWork)
{
    public Task<IReadOnlyList<State>> ListAsync(CancellationToken cancellationToken) => states.ListAsync(cancellationToken);

    /// <returns><c>false</c> when the state does not exist.</returns>
    public async Task<bool> SetNotAllowedAsync(int stateId, bool isNotAllowed, CancellationToken cancellationToken)
    {
        var state = await states.FindAsync(stateId, cancellationToken);
        if (state is null)
        {
            return false;
        }

        state.SetNotAllowed(isNotAllowed);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
