using Loans.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Loans.Infrastructure.Persistence;

/// <summary>Maps the domain to the table and column names required by the spec.</summary>
public sealed class LoansDbContext(DbContextOptions<LoansDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<State> States => Set<State>();
    public DbSet<BlacklistedSsn> Blacklist => Set<BlacklistedSsn>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var ssnConverter = new ValueConverter<Ssn, string>(ssn => ssn.Value, value => Ssn.Parse(value));

        modelBuilder.Entity<Customer>(customer =>
        {
            customer.ToTable("Customer");
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id).HasColumnName("customerId");
            customer.Property(c => c.Ssn).HasColumnName("SSN").HasConversion(ssnConverter).HasMaxLength(9).IsFixedLength().IsUnicode(false);
            customer.HasIndex(c => c.Ssn).IsUnique();

            // The ApplicantDetails value object is stored in the Customer row, in the spec's columns.
            customer.ComplexProperty(c => c.Details, details =>
            {
                details.Property(d => d.FirstName).HasColumnName("first_name").HasMaxLength(ApplicantDetails.NameMaxLength);
                details.Property(d => d.LastName).HasColumnName("last_name").HasMaxLength(ApplicantDetails.NameMaxLength);
                details.Property(d => d.Email).HasColumnName("email").HasMaxLength(ApplicantDetails.TextMaxLength);
                details.Property(d => d.AddressLine1).HasColumnName("AddressLine1").HasMaxLength(ApplicantDetails.TextMaxLength);
                details.Property(d => d.AddressLine2).HasColumnName("AddressLine2").HasMaxLength(ApplicantDetails.TextMaxLength);
                details.Property(d => d.StateId).HasColumnName("stateId");
                details.Property(d => d.ZipCode).HasColumnName("zip_code").HasMaxLength(ApplicantDetails.ZipCodeLength);
                details.Property(d => d.CompanyName).HasColumnName("company_name").HasMaxLength(ApplicantDetails.TextMaxLength);
            });
        });

        modelBuilder.Entity<LoanApplication>(loanApplication =>
        {
            loanApplication.ToTable("Application");
            loanApplication.HasKey(a => a.Id);
            loanApplication.Property(a => a.Id).HasColumnName("appId");
            loanApplication.Property(a => a.CustomerId).HasColumnName("customerId");
            loanApplication.Property(a => a.SubmittedAtUtc).HasColumnName("date_UTC");
            loanApplication.Property(a => a.RequestedAmount).HasColumnName("requested_ammount").HasPrecision(18, 2);
            loanApplication.HasOne(a => a.Customer).WithOne().HasForeignKey<LoanApplication>(a => a.CustomerId);
        });

        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("Users");
            user.HasKey(u => u.Id);
            user.Property(u => u.Id).HasColumnName("userId");
            user.Property(u => u.Email).HasColumnName("email").HasMaxLength(200);
            user.Property(u => u.PasswordHash).HasColumnName("password").HasMaxLength(200);
            user.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<State>(state =>
        {
            state.ToTable("StateStatus");
            state.HasKey(s => s.Id);
            state.Property(s => s.Id).HasColumnName("stateId").ValueGeneratedNever();
            state.Property(s => s.Name).HasColumnName("state_name").HasMaxLength(50);
            state.Property(s => s.Abbreviation).HasColumnName("Abbr").HasMaxLength(2).IsFixedLength();
            state.Property(s => s.IsNotAllowed).HasColumnName("isNotAllowed");
            state.HasData(SeedData.States);
        });

        modelBuilder.Entity<BlacklistedSsn>(entry =>
        {
            entry.ToTable("BlackList");
            entry.HasKey(b => b.Id);
            entry.Property(b => b.Id).HasColumnName("blistId");
            entry.Property(b => b.Ssn).HasColumnName("ssn").HasConversion(ssnConverter).HasMaxLength(9).IsFixedLength().IsUnicode(false);
            entry.Property(b => b.AddedAtUtc).HasColumnName("date_utc");
            entry.HasIndex(b => b.Ssn).IsUnique();
        });
    }
}
