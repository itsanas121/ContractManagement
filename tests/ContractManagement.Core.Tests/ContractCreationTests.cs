using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Tests;

public class ContractCreationTests
{
    private static (Company Company, User Owner) NewCompanyAndOwner()
    {
        var company = new Company("Test Company", "1010000001", null, null, null);
        var owner = new User("Sara", "Ali", "sara@test.com", "E001", "hash", UserRole.ContractManager, company);
        return (company, owner);
    }

    private static Contract NewContract(Company company, User owner,
        decimal value = 50000m, DateOnly? start = null, DateOnly? end = null) =>
        new("CNT-001", "Service Agreement", null, value,
            start ?? new DateOnly(2026, 11, 1), end ?? new DateOnly(2027, 10, 31),
            ContractType.Service, company, owner);

    [Fact]
    public void Contract_with_zero_value_is_rejected()
    {
        var (company, owner) = NewCompanyAndOwner();

        var exception = Assert.Throws<DomainException>(() => NewContract(company, owner, value: 0));

        Assert.Contains("value", exception.Message);
    }

    [Fact]
    public void Contract_whose_end_date_is_not_after_start_date_is_rejected()
    {
        var (company, owner) = NewCompanyAndOwner();
        var date = new DateOnly(2026, 11, 1);

        var exception = Assert.Throws<DomainException>(() => NewContract(company, owner, start: date, end: date));

        Assert.Contains("End date", exception.Message);
    }

    [Fact]
    public void Contract_for_an_inactive_company_is_rejected()
    {
        var (company, owner) = NewCompanyAndOwner();
        company.Deactivate();

        var exception = Assert.Throws<DomainException>(() => NewContract(company, owner));

        Assert.Contains("inactive company", exception.Message);
    }

    [Fact]
    public void Contract_with_an_inactive_owner_is_rejected()
    {
        var (company, owner) = NewCompanyAndOwner();
        owner.Deactivate();

        var exception = Assert.Throws<DomainException>(() => NewContract(company, owner));

        Assert.Contains("active user", exception.Message);
    }

    [Fact]
    public void Contract_with_an_owner_from_another_company_is_rejected()
    {
        var (company, owner) = NewCompanyAndOwner();
        TestData.SetProperty(owner, "CompanyId", 5);

        var exception = Assert.Throws<DomainException>(() => NewContract(company, owner));

        Assert.Contains("same company", exception.Message);
    }
}