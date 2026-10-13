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

}
