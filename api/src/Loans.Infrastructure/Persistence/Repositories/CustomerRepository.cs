using Loans.Application.Abstractions;
using Loans.Domain;
using Microsoft.EntityFrameworkCore;

namespace Loans.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(LoansDbContext db) : ICustomerRepository
{
    public Task<Customer?> FindBySsnAsync(Ssn ssn, CancellationToken cancellationToken) =>
        db.Customers.SingleOrDefaultAsync(c => c.Ssn == ssn, cancellationToken);

    public void Add(Customer customer) => db.Customers.Add(customer);
}
