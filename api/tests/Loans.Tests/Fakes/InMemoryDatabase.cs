using Loans.Application.Abstractions;
using Loans.Domain;

namespace Loans.Tests.Fakes;

/// <summary>
/// In-memory stand-in for the database. Added entities stay pending until the unit of work commits,
/// so tests can check that a rolled-back transaction leaves nothing behind.
/// </summary>
public sealed class InMemoryDatabase :
    ICustomerRepository, ILoanApplicationRepository, IStateRepository, IBlacklistRepository, IUserRepository, IUnitOfWork
{
    private readonly List<object> _pendingAdds = [];
    private readonly List<object> _pendingRemoves = [];
    private int _nextId = 100;
    private bool _inTransaction;

    public List<Customer> Customers { get; } = [];
    public List<LoanApplication> LoanApplications { get; } = [];
    public List<State> States { get; } = [];
    public List<BlacklistedSsn> Blacklist { get; } = [];
    public List<User> Users { get; } = [];
    public int Commits { get; private set; }

    /// <summary>When set, reading a state throws it, simulating a database outage.</summary>
    public Exception? ReadFailure { get; set; }
    public int Rollbacks { get; private set; }

    public Task<Customer?> FindBySsnAsync(Ssn ssn, CancellationToken cancellationToken) =>
        Task.FromResult(Customers.SingleOrDefault(c => c.Ssn == ssn));

    public void Add(Customer customer) => _pendingAdds.Add(customer);

    public Task<LoanApplication?> FindByCustomerIdAsync(int customerId, CancellationToken cancellationToken) =>
        Task.FromResult(LoanApplications.SingleOrDefault(a => a.CustomerId == customerId));

    public void Add(LoanApplication loanApplication) => _pendingAdds.Add(loanApplication);

    Task<IReadOnlyList<State>> IStateRepository.ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<State>>(States);

    public Task<State?> FindAsync(int stateId, CancellationToken cancellationToken) =>
        ReadFailure is null ? Task.FromResult(States.SingleOrDefault(s => s.Id == stateId)) : throw ReadFailure;

    Task<IReadOnlyList<BlacklistedSsn>> IBlacklistRepository.ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<BlacklistedSsn>>(Blacklist);

    public Task<BlacklistedSsn?> FindAsync(Ssn ssn, CancellationToken cancellationToken) =>
        Task.FromResult(Blacklist.SingleOrDefault(b => b.Ssn == ssn));

    public void Add(BlacklistedSsn entry) => _pendingAdds.Add(entry);

    public void Remove(BlacklistedSsn entry) => _pendingRemoves.Add(entry);

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(u => u.Email == email));

    /// <summary>Inside a transaction only assigns ids; outside one it persists immediately, like EF.</summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        if (_inTransaction)
        {
            AssignIds();
        }
        else
        {
            Commit();
        }

        return Task.CompletedTask;
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        _inTransaction = true;
        try
        {
            await work(cancellationToken);
            Commit();
            Commits++;
        }
        catch
        {
            _pendingAdds.Clear();
            _pendingRemoves.Clear();
            Rollbacks++;
            throw;
        }
        finally
        {
            _inTransaction = false;
        }
    }

    /// <summary>Moves pending changes into the committed lists.</summary>
    public void Commit()
    {
        AssignIds();
        foreach (var entity in _pendingAdds)
        {
            switch (entity)
            {
                case Customer customer: Customers.Add(customer); break;
                case LoanApplication loanApplication:
                    SetProperty(loanApplication, nameof(LoanApplication.CustomerId), loanApplication.Customer.Id);
                    LoanApplications.Add(loanApplication);
                    break;
                case BlacklistedSsn entry: Blacklist.Add(entry); break;
            }
        }

        foreach (var entry in _pendingRemoves.OfType<BlacklistedSsn>())
        {
            Blacklist.Remove(entry);
        }

        _pendingAdds.Clear();
        _pendingRemoves.Clear();
    }

    public Customer SeedCustomer(Ssn ssn, ApplicantDetails details, decimal requestedAmount, DateTime submittedAtUtc)
    {
        var customer = Customer.Register(ssn, details);
        Add(customer);
        Add(LoanApplication.Submit(customer, requestedAmount, submittedAtUtc));
        Commit();
        return customer;
    }

    /// <summary>Assigns ids to pending entities, like SQL Server identity columns do.</summary>
    private void AssignIds()
    {
        foreach (var entity in _pendingAdds.Where(e => GetId(e) == 0))
        {
            SetId(entity, _nextId++);
        }
    }

    private static int GetId(object entity) => (int)entity.GetType().GetProperty("Id")!.GetValue(entity)!;

    private static void SetId(object entity, int id) => SetProperty(entity, "Id", id);

    private static void SetProperty(object entity, string property, object value) =>
        entity.GetType().GetProperty(property)!.SetValue(entity, value);
}
