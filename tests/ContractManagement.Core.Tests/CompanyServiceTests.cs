using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
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
}
