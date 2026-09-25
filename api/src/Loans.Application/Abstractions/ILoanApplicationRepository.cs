using Loans.Domain;

namespace Loans.Application.Abstractions;

public interface ILoanApplicationRepository
{
    Task<LoanApplication?> FindByCustomerIdAsync(int customerId, CancellationToken cancellationToken);
    void Add(LoanApplication loanApplication);
}
