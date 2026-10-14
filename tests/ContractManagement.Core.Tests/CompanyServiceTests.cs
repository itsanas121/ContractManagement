using ContractManagement.Core.Application.DTOs.Companies;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using ContractManagement.Core.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Tests;


public class CompanyServiceTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeAuditService _audit = new();
    private readonly FakeCurrentUser _currentUser = new()
    {
        UserId = 7
    };

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Deactivate_active_company_saves_status_and_audit()
    {
        using var db = _database.CreateContext();

        // Arrange: Create an active company
        var company = new Company(
            "Test Company", "REG-101", null, null, null);

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new CompanyService(
            db, _audit, _currentUser);

        // Act: Deactivate the company
        await service.DeactivateAsync(company.Id);

        // Assert: Verify the database
        var saved = await db.Companies
            .AsNoTracking()
            .SingleAsync(c => c.Id == company.Id);

        Assert.Equal(CompanyStatus.Inactive, saved.Status);

        // Assert: Verify the audit
        var audit = Assert.Single(_audit.Calls);

        Assert.Equal(AuditAction.Deactivated, audit.Action);
        Assert.Equal(AuditEntityType.Company, audit.EntityType);
        Assert.Equal(company.Id, audit.EntityId);
        Assert.Equal(7, audit.PerformedById);
    }

    [Fact]
    public async Task Activate_inactive_company_saves_status_and_audit()
    {
        using var db = _database.CreateContext();

        var company = new Company("Test", "REG-101", null, null, null);
        company.Deactivate();

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new CompanyService(db, _audit, _currentUser);

        await service.ActivateAsync(company.Id);

        Assert.True(company.IsActive);
        Assert.Equal(AuditAction.Activated, Assert.Single(_audit.Calls).Action);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Invalid_company_status_change_is_rejected(bool activate)
    {
        using var db = _database.CreateContext();

        var company = new Company("Test", "REG-101", null, null, null);

        if (!activate)
            company.Deactivate();

        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var service = new CompanyService(db, _audit, _currentUser);

        await Assert.ThrowsAsync<DomainException>(() =>
            activate
                ? service.ActivateAsync(company.Id)
                : service.DeactivateAsync(company.Id));

        Assert.Empty(_audit.Calls);
    }

    [Fact]
    public async Task Duplicate_company_registration_is_rejected()
    {
        using var db = _database.CreateContext();

        var service = new CompanyService(db, _audit, _currentUser);

        await service.CreateAsync(new CreateCompanyRequest
        {
            Name = "Company A",
            RegistrationNumber = "REG-101"
        });

        await Assert.ThrowsAsync<DomainException>(() =>
            service.CreateAsync(new CreateCompanyRequest
            {
                Name = "Company B",
                RegistrationNumber = "REG-101"
            }));

        Assert.Equal(1, await db.Companies.CountAsync());
    }
}


