using Loans.Application.Abstractions;
using Loans.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Loans.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private const int MaxConnectAttempts = 20;
    private static readonly TimeSpan ConnectRetryDelay = TimeSpan.FromSeconds(3);

    /// <summary>Creates the schema (tables + state seed) and the demo data. Waits for SQL Server to boot.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoansDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<LoansDbContext>>();

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
                break;
            }
            catch (Exception exception) when (attempt < MaxConnectAttempts && exception is not OperationCanceledException)
            {
                logger.LogInformation("Database not ready (attempt {Attempt}); retrying in {Delay}.", attempt, ConnectRetryDelay);
                await Task.Delay(ConnectRetryDelay, cancellationToken);
            }
        }

        var nowUtc = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;
        await SeedDemoDataAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher>(), nowUtc, cancellationToken);
    }

    private static async Task SeedDemoDataAsync(
        LoansDbContext db, IPasswordHasher passwordHasher, DateTime nowUtc, CancellationToken cancellationToken)
    {
        if (!await db.Users.AnyAsync(cancellationToken))
        {
            db.Users.Add(new User(SeedData.AdminEmail, passwordHasher.Hash(SeedData.AdminPassword)));
        }

        if (!await db.Blacklist.AnyAsync(cancellationToken))
        {
            db.Blacklist.AddRange(SeedData.BlacklistedSsns.Select(ssn => new BlacklistedSsn(Ssn.Parse(ssn), nowUtc)));
        }

        if (!await db.Customers.AnyAsync(cancellationToken))
        {
            var customer = Customer.Register(
                Ssn.Parse(SeedData.ReturningCustomerSsn),
                new ApplicantDetails("Jane", "Doe", "jane.doe@example.com", "100 Main St", null, SeedData.CaliforniaStateId, "90001", "Doe Consulting"));
            db.Customers.Add(customer);
            db.LoanApplications.Add(LoanApplication.Submit(customer, 5_000m, nowUtc));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
