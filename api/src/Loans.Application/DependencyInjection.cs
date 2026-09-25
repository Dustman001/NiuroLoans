using Loans.Application.Authentication;
using Loans.Application.Configuration;
using Loans.Application.LoanApplications;
using Loans.Application.LoanApplications.Rules;
using Microsoft.Extensions.DependencyInjection;

namespace Loans.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Rules run in this order. To add a rule, implement ILoanRule and register it here.
        services.AddScoped<ILoanRule, StateAllowedRule>();
        services.AddScoped<ILoanRule, SsnNotBlacklistedRule>();
        services.AddScoped<LoanRuleEngine>();

        services.AddScoped<LoanApplicationService>();
        services.AddScoped<StateService>();
        services.AddScoped<BlacklistService>();
        services.AddScoped<AuthService>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
