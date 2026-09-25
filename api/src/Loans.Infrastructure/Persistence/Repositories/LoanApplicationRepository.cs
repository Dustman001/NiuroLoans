using Loans.Application.Abstractions;
using Loans.Domain;
using Microsoft.EntityFrameworkCore;

namespace Loans.Infrastructure.Persistence.Repositories;

public sealed class LoanApplicationRepository(LoansDbContext db) : ILoanApplicationRepository
{
    public Task<LoanApplication?> FindByCustomerIdAsync(int customerId, CancellationToken cancellationToken) =>
        db.LoanApplications.SingleOrDefaultAsync(a => a.CustomerId == customerId, cancellationToken);

    public void Add(LoanApplication loanApplication) => db.LoanApplications.Add(loanApplication);
}
