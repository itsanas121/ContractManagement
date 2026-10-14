using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace ContractManagement.Api.Seeding;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {

        using var scope = serviceProvider.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DevelopmentDataSeeder));

        var environment = scope.ServiceProvider
            .GetRequiredService<IHostEnvironment>();

        if (!environment.IsDevelopment())
            return;

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<User>>();

        // Safety: this seed is only for SQL server localdb
        var dataSource = db.Database.GetDbConnection().DataSource;

        if (!dataSource.StartsWith(
            "(localdb)\\",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Development seeding requires a local LocalDB database.");
        }

        // Read companies, users and password from configuration
        var options = configuration
            .GetSection("SeedData")
            .Get<SeedDataOptions>()
            ?? throw new InvalidOperationException(
                "SeedData Configuration is missing");

        if (string.IsNullOrWhiteSpace(options.Password))
            throw new InvalidOperationException(
                "Configure SeedData:Password in User Secrets.");

        if (options.Companies.Count == 0 ||
            options.Users.Count == 0)
            throw new InvalidOperationException(
                "Configure at least one company and one user.");

        // Apply EF Core migrations
        await db.Database.MigrateAsync();

        // Both company and user inserts succeed together, or neither is commited
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        // Seed companies
        var companies = new Dictionary<string, Company>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var item in options.Companies)
        {
            if (string.IsNullOrWhiteSpace(item.RegistrationNumber))
                throw new InvalidOperationException(
                    "Company registration number is required.");

            if (companies.ContainsKey(item.RegistrationNumber))
                throw new InvalidOperationException(
                    $"Duplicate seed company: {item.RegistrationNumber}");

            var company = await db.Companies
                .SingleOrDefaultAsync(c =>
                c.RegistrationNumber == item.RegistrationNumber);

            if (company is null)
            {
                company = new Company(
                    item.Name,
                    item.RegistrationNumber,
                    item.Email,
                    item.Phone,
                    item.Address);
                db.Companies.Add(company);
            }

            if (!company.IsActive)
            {
                logger.LogWarning(
                    "Seed company {RegistrationNumber} is inactive. Preserving its current status.",
                    item.RegistrationNumber);
            }

            companies.Add(item.RegistrationNumber, company);
        }

        // Save companies first to generate their database IDs.
        await db.SaveChangesAsync();

        // Seed users
        var emails = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var item in options.Users)
        {
            if (string.IsNullOrWhiteSpace(item.Email) ||
                !emails.Add(item.Email))
                throw new InvalidOperationException(
                    $"Invalid or duplicate seed email: {item.Email}");

            if (string.IsNullOrWhiteSpace(item.CompanyRegistrationNumber) ||
                !companies.TryGetValue(
                    item.CompanyRegistrationNumber,
                    out var company))
                throw new InvalidOperationException(
                    $"Company not configured for user {item.Email}.");

            if (!Enum.TryParse<UserRole>(
                item.Role, true, out var role) ||
                !Enum.IsDefined(role))
                throw new InvalidOperationException(
                    $"invalid role for user {item.Email}");


            var existingUser = await db.Users
                .SingleOrDefaultAsync(u => u.Email == item.Email);


            if (existingUser is not null)
            {
                if (existingUser.Role != role ||
                    existingUser.CompanyId != company.Id ||
                    existingUser.EmployeeNumber != item.EmployeeNumber ||
                    !existingUser.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Existing user {item.Email} " +
                        "does not match the seed configuration.");
                }
                continue;
            }

            //temp object used only for hasing the password
            var temporaryUser = new User(
                item.FirstName,
                item.LastName,
                item.Email,
                item.EmployeeNumber,
                "temporary-placeholder", //dummy value
                role,
                company);

            var passwordHash = passwordHasher.HashPassword(
                temporaryUser,
                options.Password);

            //Create the real user
            var dummyUser = new User(
                item.FirstName,
                item.LastName,
                item.Email,
                item.EmployeeNumber,
                passwordHash,
                role,
                company
                );

            db.Users.Add(dummyUser);
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }


}
