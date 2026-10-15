using ContractManagement.Core.Application.DTOs.Users;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using ContractManagement.Core.Tests.Fakes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Tests;

public class UserServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeAuditService _audit = new();
    private readonly FakeCurrentUser _currentUser =
        new() { UserId = 1 };

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Register_user_saves_password_hash_and_audit()
    {
        using var db = _database.CreateContext();

        var company = new Company(
            "Test Company", "TEST-001", null, null, null);

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var hasher = new PasswordHasher<User>();

        var service = new UserService(
            db, _audit, _currentUser, hasher);

        var request = new CreateUserRequest
        {
            FirstName = "Ahmed",
            LastName = "Ali",
            Email = "ahmed@test.com",
            EmployeeNumber = "EMP-001",
            Password = "TestPassword123!",
            Role = UserRole.ContractManager,
            CompanyId = company.Id
        };

        var result = await service.CreateAsync(request);

        var user = await db.Users.SingleAsync();

        Assert.Equal(user.Id, result.Id);

        // Password must be hashed, not stored as plain text
        Assert.NotEqual(request.Password, user.PasswordHash);

        // Verify password hash
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password));

        // Verify audit logging
        Assert.Single(_audit.Calls);
        Assert.Equal(
            AuditAction.Created,
            _audit.Calls[0].Action);
    }


    [Fact]
    public async Task Register_user_with_duplicate_email_is_rejected()
    {
        // Arrange
        using var db = _database.CreateContext();

        var company = new Company(
            "Test Company", "TEST-001", null, null, null);

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new UserService(
            db,
            _audit,
            _currentUser,
            new PasswordHasher<User>());

        var request = new CreateUserRequest
        {
            FirstName = "Ahmed",
            LastName = "Ali",
            Email = "ahmed@test.com",
            EmployeeNumber = "EMP-001",
            Password = "TestPassword123!",
            Role = UserRole.ContractManager,
            CompanyId = company.Id
        };

        // Register the first user successfully
        await service.CreateAsync(request);

        // Act: Register another user with the same email
        request.EmployeeNumber = "EMP-002";

        await Assert.ThrowsAsync<DomainException>(
            () => service.CreateAsync(request));

        Assert.Single(await db.Users.ToListAsync());

        Assert.Single(_audit.Calls);
    }


    [Fact]
    public async Task Register_user_under_inactive_company_is_rejected()
    {
        // Arrange
        using var db = _database.CreateContext();

        var company = new Company(
            "Test Company", "TEST-001", null, null, null);

        company.Deactivate();

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new UserService(
            db,
            _audit,
            _currentUser,
            new PasswordHasher<User>());

        var request = new CreateUserRequest
        {
            FirstName = "Ahmed",
            LastName = "Ali",
            Email = "ahmed@test.com",
            EmployeeNumber = "EMP-001",
            Password = "TestPassword123!",
            Role = UserRole.ContractManager,
            CompanyId = company.Id
        };

        // Act: Registration must be rejected
        await Assert.ThrowsAsync<DomainException>(
            () => service.CreateAsync(request));

        // Assert: No user was saved
        Assert.Empty(await db.Users.ToListAsync());

        // No audit entry should be created
        Assert.Empty(_audit.Calls);
    }


    [Fact]
    public async Task Deactivate_and_reactivate_user_successfully()
    {
        // Arrange
        using var db = _database.CreateContext();

        var company = new Company(
            "Test Company", "TEST-001", null, null, null);

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new UserService(
            db,
            _audit,
            _currentUser,
            new PasswordHasher<User>());

        var request = new CreateUserRequest
        {
            FirstName = "Ahmed",
            LastName = "Ali",
            Email = "ahmed@test.com",
            EmployeeNumber = "EMP-001",
            Password = "TestPassword123!",
            Role = UserRole.ContractManager,
            CompanyId = company.Id
        };

        var createdUser = await service.CreateAsync(request);

        // Act 1: Deactivate the user
        await service.DeactivateAsync(
            createdUser.Id, CancellationToken.None);

        // Assert: User is inactive in the database
        var inactiveUser = await db.Users
            .AsNoTracking()
            .SingleAsync(u => u.Id == createdUser.Id);

        Assert.Equal(UserStatus.Inactive, inactiveUser.Status);

        // Act 2: Reactivate the user
        await service.ActivateAsync(
            createdUser.Id, CancellationToken.None);

        // Assert: User is active again
        var activeUser = await db.Users
            .AsNoTracking()
            .SingleAsync(u => u.Id == createdUser.Id);

        Assert.Equal(UserStatus.Active, activeUser.Status);

        // Assert: All three actions were audited
        Assert.Equal(3, _audit.Calls.Count);

        Assert.Equal(
            AuditAction.Created, _audit.Calls[0].Action);

        Assert.Equal(
            AuditAction.Deactivated, _audit.Calls[1].Action);

        Assert.Equal(
            AuditAction.Activated, _audit.Calls[2].Action);
    }


    [Fact]
    public async Task GetAll_returns_users_with_profiles()
    {
        // Arrange
        using var db = _database.CreateContext();

        var company = new Company(
            "Test Company", "TEST-001", null, null, null);

        var zaid = new User(
            "Zaid", "Ali", "zaid@test.com", "EMP-002",
            "test-hash", UserRole.ContractManager, company);

        var ahmed = new User(
            "Ahmed", "Ali", "ahmed@test.com", "EMP-001",
            "test-hash", UserRole.Reviewer, company);

        ahmed.SetProfile("Developer", "IT", "0500000000");

        db.Users.AddRange(zaid, ahmed);
        await db.SaveChangesAsync();

        var service = new UserService(
            db, _audit, _currentUser,
            new PasswordHasher<User>());

        // Act
        var users = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, users.Count);

        var ahmedResult = Assert.Single(users, u => u.FirstName == "Ahmed");
        var zaidResult = Assert.Single(users, u => u.FirstName == "Zaid");

        Assert.Equal("Developer", ahmed.Profile?.JobTitle);
        Assert.Equal("IT", ahmed.Profile?.Department);
        Assert.Null(zaid.Profile);
    }

    [Fact]
    public async Task Duplicate_employee_number_in_same_company_is_rejected()
    {
        using var db = _database.CreateContext();

        var company = new Company(
            "Test Company", "TEST-001", null, null, null);

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new UserService(
            db, _audit, _currentUser,
            new PasswordHasher<User>());

        var request = new CreateUserRequest
        {
            FirstName = "Ahmed",
            LastName = "Ali",
            Email = "ahmed@test.com",
            EmployeeNumber = "EMP-001",
            Password = "TestPassword123!",
            Role = UserRole.ContractManager,
            CompanyId = company.Id
        };

        await service.CreateAsync(request);

        // Different email, but same employee number and company
        request.Email = "sara@test.com";
        request.FirstName = "Sara";

        await Assert.ThrowsAsync<DomainException>(
            () => service.CreateAsync(request));

        Assert.Equal(1, await db.Users.CountAsync());
    }


    [Fact]
    public async Task Same_employee_number_in_different_companies_is_allowed()
    {
        using var db = _database.CreateContext();

        var companyA = new Company(
            "Company A", "REG-001", null, null, null);

        var companyB = new Company(
            "Company B", "REG-002", null, null, null);

        db.Companies.AddRange(companyA, companyB);
        await db.SaveChangesAsync();

        var service = new UserService(
            db, _audit, _currentUser,
            new PasswordHasher<User>());

        var request = new CreateUserRequest
        {
            FirstName = "Ahmed",
            LastName = "Ali",
            Email = "ahmed@test.com",
            EmployeeNumber = "EMP-001",
            Password = "TestPassword123!",
            Role = UserRole.ContractManager,
            CompanyId = companyA.Id
        };

        await service.CreateAsync(request);

        // Same employee number, different company and email
        request.Email = "sara@test.com";
        request.FirstName = "Sara";
        request.CompanyId = companyB.Id;

        await service.CreateAsync(request);

        Assert.Equal(2, await db.Users.CountAsync());
    }

    [Theory]
    [InlineData("FirstName")]
    [InlineData("LastName")]
    [InlineData("Email")]
    [InlineData("EmployeeNumber")]
    public async Task Register_user_with_missing_required_field_is_rejected(
        string missingField)
    {
        using var db = _database.CreateContext();

        var company = new Company(
            "Test Company", "TEST-001", null, null, null);

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new UserService(
            db, _audit, _currentUser,
            new PasswordHasher<User>());

        var request = new CreateUserRequest
        {
            FirstName = "Ahmed",
            LastName = "Ali",
            Email = "ahmed@test.com",
            EmployeeNumber = "EMP-001",
            Password = "TestPassword123!",
            Role = UserRole.ContractManager,
            CompanyId = company.Id
        };

        // Make one required field empty per test case
        switch (missingField)
        {
            case "FirstName":
                request.FirstName = "";
                break;

            case "LastName":
                request.LastName = "";
                break;

            case "Email":
                request.Email = "";
                break;

            case "EmployeeNumber":
                request.EmployeeNumber = "";
                break;
        }

        await Assert.ThrowsAsync<DomainException>(
            () => service.CreateAsync(request));

        Assert.Empty(await db.Users.ToListAsync());
        Assert.Empty(_audit.Calls);
    }

}
