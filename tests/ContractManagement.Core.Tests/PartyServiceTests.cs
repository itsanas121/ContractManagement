using System.Text.Json;
using ContractManagement.Core.Application.DTOs.Parties;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;
using ContractManagement.Core.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Core.Tests;

public class PartyServiceTests : IDisposable
{
    private const int CurrentUserId = 7;

    private readonly TestDatabase _database = new();
    private readonly FakeAuditService _audit = new();
    private readonly FakeCurrentUser _currentUser = new() { UserId = CurrentUserId };

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Register_individual_with_valid_data_returns_response_and_saves_party()
    {
        var response = await RegisterIndividual(IndividualRequest());

        Assert.True(response.Id > 0);
        Assert.Equal("Individual", response.PartyType);
        Assert.Equal("Ahmed Ali", response.Name);
        Assert.Equal("ahmed@test.com", response.Email);
        Assert.Equal("0500000000", response.Phone);
        Assert.Equal("1012345678", response.NationalId);
        Assert.Null(response.RegistrationNumber);
        Assert.Null(response.ContactPerson);

        var saved = Assert.IsType<IndividualParty>(Assert.Single(await LoadSavedParties()));
        Assert.Equal(response.Id, saved.Id);
        Assert.Equal("1012345678", saved.NationalId);
    }

    [Fact]
    public async Task Register_organization_with_valid_data_returns_response_and_saves_party()
    {
        var response = await RegisterOrganization(OrganizationRequest());

        Assert.True(response.Id > 0);
        Assert.Equal("Organization", response.PartyType);
        Assert.Equal("ABC Company", response.Name);
        Assert.Equal("info@abc.com", response.Email);
        Assert.Equal("0112345678", response.Phone);
        Assert.Equal("7001234567", response.RegistrationNumber);
        Assert.Equal("Sara Ali", response.ContactPerson);
        Assert.Null(response.NationalId);

        var saved = Assert.IsType<OrganizationParty>(Assert.Single(await LoadSavedParties()));
        Assert.Equal(response.Id, saved.Id);
        Assert.Equal("ABC Company", saved.Name);
    }

    [Fact]
    public async Task Register_individual_with_duplicate_national_id_is_rejected()
    {
        await RegisterIndividual(IndividualRequest(name: "First"));

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            RegisterIndividual(IndividualRequest(name: "Second")));

        Assert.Equal("An individual party with the same National ID already exists.", exception.Message);
        Assert.Single(await LoadSavedParties());
        Assert.Single(_audit.Calls);
    }

    [Fact]
    public async Task Register_organization_with_duplicate_registration_number_is_rejected()
    {
        await RegisterOrganization(OrganizationRequest(organizationName: "First"));

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            RegisterOrganization(OrganizationRequest(organizationName: "Second")));

        Assert.Equal("An organization party with the same registration number already exists.", exception.Message);
        Assert.Single(await LoadSavedParties());
        Assert.Single(_audit.Calls);
    }

    [Fact]
    public async Task Organization_registration_number_may_equal_an_individual_national_id()
    {
        await RegisterIndividual(IndividualRequest(nationalId: "1012345678"));

        var response = await RegisterOrganization(OrganizationRequest(registrationNumber: "1012345678"));

        Assert.Equal("Organization", response.PartyType);
        Assert.Equal(2, (await LoadSavedParties()).Count);
    }

    [Fact]
    public async Task Register_individual_trims_the_input()
    {
        var response = await RegisterIndividual(IndividualRequest(
            name: "  Ali  ", nationalId: " 1012345678 ", email: " ali@test.com ", phone: " 0500000000 "));

        Assert.Equal("Ali", response.Name);
        Assert.Equal("1012345678", response.NationalId);
        Assert.Equal("ali@test.com", response.Email);
        Assert.Equal("0500000000", response.Phone);

        var saved = Assert.IsType<IndividualParty>(Assert.Single(await LoadSavedParties()));
        Assert.Equal("Ali", saved.Name);
        Assert.Equal("1012345678", saved.NationalId);
        Assert.Equal("ali@test.com", saved.Email);
        Assert.Equal("0500000000", saved.Phone);
    }

    [Fact]
    public async Task Register_organization_trims_the_input()
    {
        var response = await RegisterOrganization(OrganizationRequest(
            organizationName: "  ABC  ", registrationNumber: " 7001234567 ",
            contactPerson: "  Sara  ", email: " info@abc.com ", phone: " 0112345678 "));

        Assert.Equal("ABC", response.Name);
        Assert.Equal("7001234567", response.RegistrationNumber);
        Assert.Equal("Sara", response.ContactPerson);

        var saved = Assert.IsType<OrganizationParty>(Assert.Single(await LoadSavedParties()));
        Assert.Equal("ABC", saved.Name);
        Assert.Equal("7001234567", saved.RegistrationNumber);
        Assert.Equal("Sara", saved.ContactPerson);
        Assert.Equal("info@abc.com", saved.Email);
        Assert.Equal("0112345678", saved.Phone);
    }

    [Fact]
    public async Task Register_individual_writes_one_audit_entry()
    {
        var response = await RegisterIndividual(IndividualRequest());

        var call = Assert.Single(_audit.Calls);
        Assert.Equal(AuditEntityType.Party, call.EntityType);
        Assert.Equal(response.Id, call.EntityId);
        Assert.Equal(AuditAction.Created, call.Action);
        Assert.Equal(CurrentUserId, call.PerformedById);
        Assert.Null(call.OldValue);
        AssertAuditPayload(call.NewValue, "Individual", "Ahmed Ali");
    }

    [Fact]
    public async Task Register_organization_writes_one_audit_entry()
    {
        var response = await RegisterOrganization(OrganizationRequest());

        var call = Assert.Single(_audit.Calls);
        Assert.Equal(AuditEntityType.Party, call.EntityType);
        Assert.Equal(response.Id, call.EntityId);
        Assert.Equal(AuditAction.Created, call.Action);
        Assert.Equal(CurrentUserId, call.PerformedById);
        AssertAuditPayload(call.NewValue, "Organization", "ABC Company");
    }

    [Fact]
    public async Task Register_individual_with_invalid_data_saves_nothing_and_writes_no_audit()
    {
        await Assert.ThrowsAsync<DomainValidationException>(() =>
            RegisterIndividual(IndividualRequest(nationalId: "123")));

        Assert.Empty(await LoadSavedParties());
        Assert.Empty(_audit.Calls);
    }

    [Fact]
    public async Task Register_organization_with_invalid_data_saves_nothing_and_writes_no_audit()
    {
        await Assert.ThrowsAsync<DomainValidationException>(() =>
            RegisterOrganization(OrganizationRequest(contactPerson: "   ")));

        Assert.Empty(await LoadSavedParties());
        Assert.Empty(_audit.Calls);
    }

    [Fact]
    public async Task Audit_failure_rolls_back_the_saved_party()
    {
        _audit.ThrowOnLog = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => RegisterIndividual(IndividualRequest()));

        Assert.Empty(await LoadSavedParties());
    }

    [Fact]
    public async Task GetAll_returns_both_kinds_ordered_by_name_then_id()
    {
        var zed = new IndividualParty("Zed", "1000000001", "zed@test.com", "0500000001");
        var acme = new OrganizationParty("Acme Trading", "7000000001", "Sara Ali", "info@acme.com", "0112345678");
        var aliFirst = new IndividualParty("Ali", "1000000002", "ali1@test.com", "0500000002");
        var aliSecond = new IndividualParty("Ali", "1000000003", "ali2@test.com", "0500000003");
        SeedParties(zed, acme, aliFirst, aliSecond);

        using var context = _database.CreateContext();
        var result = await new PartyService(context, _audit, _currentUser).GetAllAsync();

        Assert.Equal(
            new[] { acme.Id, aliFirst.Id, aliSecond.Id, zed.Id },
            result.Select(r => r.Id));

        var organization = result[0];
        Assert.Equal("Organization", organization.PartyType);
        Assert.Equal("Acme Trading", organization.Name);
        Assert.Null(organization.NationalId);
        Assert.Equal("7000000001", organization.RegistrationNumber);
        Assert.Equal("Sara Ali", organization.ContactPerson);

        var individual = result[1];
        Assert.Equal("Individual", individual.PartyType);
        Assert.Equal("1000000002", individual.NationalId);
        Assert.Null(individual.RegistrationNumber);
        Assert.Null(individual.ContactPerson);
    }

    [Fact]
    public async Task GetAll_returns_empty_list_when_no_parties_exist()
    {
        using var context = _database.CreateContext();

        var result = await new PartyService(context, _audit, _currentUser).GetAllAsync();

        Assert.Empty(result);
    }

    private async Task<PartyResponse> RegisterIndividual(CreateIndividualPartyRequest request)
    {
        using var context = _database.CreateContext();
        return await new PartyService(context, _audit, _currentUser).RegisterIndividualAsync(request);
    }

    private async Task<PartyResponse> RegisterOrganization(CreateOrganizationPartyRequest request)
    {
        using var context = _database.CreateContext();
        return await new PartyService(context, _audit, _currentUser).RegisterOrganizationAsync(request);
    }

    private async Task<List<Party>> LoadSavedParties()
    {
        using var context = _database.CreateContext();
        return await context.Parties.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
    }

    private void SeedParties(params Party[] parties)
    {
        using var context = _database.CreateContext();
        foreach (var party in parties)
        {
            context.Parties.Add(party);
            context.SaveChanges();
        }
    }

    private static CreateIndividualPartyRequest IndividualRequest(
        string name = "Ahmed Ali", string nationalId = "1012345678",
        string email = "ahmed@test.com", string phone = "0500000000") =>
        new() { Name = name, NationalId = nationalId, Email = email, Phone = phone };

    private static CreateOrganizationPartyRequest OrganizationRequest(
        string organizationName = "ABC Company", string registrationNumber = "7001234567",
        string contactPerson = "Sara Ali", string email = "info@abc.com", string phone = "0112345678") =>
        new()
        {
            OrganizationName = organizationName,
            RegistrationNumber = registrationNumber,
            ContactPerson = contactPerson,
            Email = email,
            Phone = phone
        };

    private static void AssertAuditPayload(object? newValue, string partyType, string name)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(newValue));
        Assert.Equal(partyType, document.RootElement.GetProperty("PartyType").GetString());
        Assert.Equal(name, document.RootElement.GetProperty("Name").GetString());
    }
}