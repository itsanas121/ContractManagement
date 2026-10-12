using ContractManagement.Core.Domain.Entities;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Tests;

public class PartyEntityTests
{
    [Fact]
    public void Individual_party_with_valid_values_is_created()
    {
        var party = new IndividualParty("Ahmed Ali", "1012345678", "ahmed@test.com", "0500000000");

        Assert.Equal("Ahmed Ali", party.Name);
        Assert.Equal("1012345678", party.NationalId);
        Assert.Equal("ahmed@test.com", party.Email);
        Assert.Equal("0500000000", party.Phone);
        Assert.NotEqual(default, party.CreatedDate);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("10123456a8")]
    [InlineData("1012 45678")]
    [InlineData("")]
    public void Individual_party_with_invalid_national_id_is_rejected(string nationalId)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new IndividualParty("Ahmed Ali", nationalId, "ahmed@test.com", "0500000000"));

        Assert.Contains("National ID must be exactly 10 digits.", exception.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Individual_party_with_blank_name_is_rejected(string name)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new IndividualParty(name, "1012345678", "ahmed@test.com", "0500000000"));

        Assert.Contains("Name is required.", exception.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Organization_party_with_blank_name_is_rejected(string name)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new OrganizationParty(name, "7001234567", "Sara Ali", "info@abc.com", "0112345678"));

        Assert.Contains("Name is required.", exception.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ahmed.test.com")]
    public void Party_with_invalid_email_is_rejected(string email)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new IndividualParty("Ahmed Ali", "1012345678", email, "0500000000"));

        Assert.Contains("A valid email is required.", exception.Errors);
    }

    [Fact]
    public void Party_with_blank_phone_is_rejected()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new IndividualParty("Ahmed Ali", "1012345678", "ahmed@test.com", " "));

        Assert.Contains("Phone is required.", exception.Errors);
    }

    [Fact]
    public void Individual_party_reports_all_errors_together()
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new IndividualParty("", "123", "bad", ""));

        Assert.Equal(4, exception.Errors.Count);
        Assert.Contains("Name is required.", exception.Errors);
        Assert.Contains("A valid email is required.", exception.Errors);
        Assert.Contains("Phone is required.", exception.Errors);
        Assert.Contains("National ID must be exactly 10 digits.", exception.Errors);
    }

    [Fact]
    public void Organization_party_with_valid_values_is_created()
    {
        var party = new OrganizationParty("ABC Company", "7001234567", "Sara Ali", "info@abc.com", "0112345678");

        Assert.Equal("ABC Company", party.Name);
        Assert.Equal("7001234567", party.RegistrationNumber);
        Assert.Equal("Sara Ali", party.ContactPerson);
        Assert.Equal("info@abc.com", party.Email);
        Assert.Equal("0112345678", party.Phone);
        Assert.NotEqual(default, party.CreatedDate);
    }

    [Theory]
    [InlineData("700123456")]
    [InlineData("70012345678")]
    [InlineData("70012345ab")]
    [InlineData("")]
    public void Organization_party_with_invalid_registration_number_is_rejected(string registrationNumber)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new OrganizationParty("ABC Company", registrationNumber, "Sara Ali", "info@abc.com", "0112345678"));

        Assert.Contains("Registration number must be exactly 10 digits.", exception.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Organization_party_with_blank_contact_person_is_rejected(string contactPerson)
    {
        var exception = Assert.Throws<DomainValidationException>(() =>
            new OrganizationParty("ABC Company", "7001234567", contactPerson, "info@abc.com", "0112345678"));

        Assert.Contains("Contact person is required.", exception.Errors);
    }
}