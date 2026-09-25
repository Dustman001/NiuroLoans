using Loans.Application.Abstractions;
using Loans.Domain;
using Loans.Infrastructure.Security;
using Loans.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Loans.Tests;

/// <summary>Runs the real API with in-memory fakes instead of SQL Server and the events service.</summary>
public sealed class LoansApiFactory : WebApplicationFactory<Program>
{
    public InMemoryDatabase Database { get; } = new();
    public FakeCustomerEventPublisher Publisher { get; } = new();

    public LoansApiFactory()
    {
        Database.States.AddRange(TestData.States());
        Database.Blacklist.Add(new BlacklistedSsn(TestData.BlacklistedSsn, DateTime.UtcNow));
        Database.Users.Add(new User(TestData.AdminEmail, new AspNetPasswordHasher().Hash(TestData.AdminPassword)));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Database:InitializeOnStartup", "false");
        builder.ConfigureTestServices(services =>
        {
            Replace<ICustomerRepository>(services, Database);
            Replace<ILoanApplicationRepository>(services, Database);
            Replace<IStateRepository>(services, Database);
            Replace<IBlacklistRepository>(services, Database);
            Replace<IUserRepository>(services, Database);
            Replace<IUnitOfWork>(services, Database);
            Replace<ICustomerEventPublisher>(services, Publisher);
        });
    }

    private static void Replace<TService>(IServiceCollection services, TService implementation) where TService : class
    {
        services.RemoveAll<TService>();
        services.AddSingleton(implementation);
    }
}
