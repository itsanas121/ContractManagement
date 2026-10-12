using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.NativeInterop;
// using ContractManagement.Domain.Entities; // <<-- uncomment/replace with the actual namespace that defines `User`

namespace ContractManagement.Api.Seeding;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();

        var environment = scope.ServiceProvider
            .GetRequiredService<IHostEnvironment>();

        if (!environment.IsDevelopment())
            return;

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<User>>();

        var adminEmail = configuration["BootStrap:AdminEmail"]?.Trim();
        var adminPassword = configuration["BootStrap:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Configure Bootstrap:AdminEmail and " +
                "Bootstrap:AdminPassword in user secrets.");
        }

        // Safety: this seed is only for SQL server localdb
        var dataSource = db.Database.GetDbConnection().DataSource;

        if (!dataSource.StartsWith(
            "localdb)\\",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Development seeding requires a local LocalDB database.");
        }

        // Apply existing EF Core migrations
        await db.Database.MigrateAsync();

        // Find or create the development company
        const string registrationNumber = "DEV-CMS-001";

        var company = await db.Companies
            .SingleOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);

        if (company is null)
        {
            company = new Company(
                name: "CMS Development Company",
                registrationNumber: registrationNumber,
                null, null, null
            );
            db.Companies.Add(company);
        }

        if (!company.IsActive)
            throw new InvalidOperationException(
                "Bootstrap company must be active.");

        // Define the three development users
        var users = new[]
        {
            (
            Email: adminEmail,
            FirstName: "Development",
            LastName: "Admin",
            EmployeeNumber: "DEV-ADMIN-01",
            Role: UserRole.Administrator
            ),
            (
            Email: adminEmail,
            FirstName: "Development",
            LastName: "Manager",
            EmployeeNumber: "DEV-MANAGER-01",
            Role: UserRole.ContractManager
            ),
            (
            Email: adminEmail,
            FirstName: "Development",
            LastName: "Admin",
            EmployeeNumber: "DEV-REVIEWER-01",
            Role: UserRole.Reviewer
            ),
        };

        foreach (var user in users)
        {
            var existingUser = await db.Users
                .SingleOrDefaultAsync(u => u.Email == user.Email);

            if (existingUser is not null)
            {
                if (existingUser.Role != user.Role ||
                        !existingUser.IsActive ||
                        existingUser.CompanyId != company.Id)
                {
                    throw new InvalidOperationException(
                        $"Existing user {existingUser.Email} "+
                        "dose not match the expected seed data."
                }
                continue;
            }

            //temp object used only for hasing the password
            var temporaryUser = new User(
                user.FirstName,
                user.LastName,
                user.Email,
                user.EmployeeNumber,
                "temporary-placeholder", //dummy value
                user.Role,
                company);

            var passwordHash = passwordHasher.HashPassword(
                temporaryUser,
                adminPassword);

            //Create the real user
            var dummyUser = new User(
                user.FirstName,
                user.LastName,
                user.Email,
                user.EmployeeNumber,
                passwordHash,
                user.Role,
                company
                );

            db.Users.Add(dummyUser)
        }

        await db.SaveChangesAsync();
    }
}




