using Loans.Application.LoanApplications;

namespace Loans.Application.Abstractions;

/// <summary>Sends customer events to the external service. Must throw when delivery fails.</summary>
public interface ICustomerEventPublisher
{
    Task PublishAsync(CustomerEvent customerEvent, CancellationToken cancellationToken);
}
