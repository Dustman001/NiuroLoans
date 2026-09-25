using Loans.Application.Abstractions;
using Loans.Infrastructure.Events;
using Loans.Infrastructure.Persistence;
using Loans.Infrastructure.Persistence.Repositories;
using Loans.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Loans.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // No EnableRetryOnFailure: it cannot wrap user transactions. The application retries the whole unit instead.
        services.AddDbContext<LoansDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Loans")));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
        services.AddScoped<IStateRepository, StateRepository>();
        services.AddScoped<IBlacklistRepository, BlacklistRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton(configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt settings are not configured."));
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();

        services.AddHttpClient<ICustomerEventPublisher, HttpCustomerEventPublisher>(client =>
        {
            var baseUrl = configuration["EventsApi:BaseUrl"] ?? throw new InvalidOperationException("EventsApi:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
