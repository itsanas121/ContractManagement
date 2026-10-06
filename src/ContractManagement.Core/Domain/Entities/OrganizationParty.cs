namespace ContractManagement.Api.Domain.Entities;

public class OrganizationParty : Party
{
    public string OrganizationName { get; private set; } = string.Empty;
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string ContactPerson { get; private set; } = string.Empty;

    private OrganizationParty() { } // Required by EF Core

    public OrganizationParty(string organizationName, string registrationNumber,
        string contactPerson, string email, string phone)
        : base(email, phone)
    {
        OrganizationName = organizationName;
        RegistrationNumber = registrationNumber;
        ContactPerson = contactPerson;
        EnsureValid();
    }

    public override string DisplayName => OrganizationName;

    public override List<string> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(OrganizationName))
            errors.Add("Organization name is required.");
        if (RegistrationNumber.Length != 10 || !RegistrationNumber.All(char.IsDigit))
            errors.Add("Registration number must be exactly 10 digits.");
        if (string.IsNullOrWhiteSpace(ContactPerson))
            errors.Add("Contact person is required.");

        return errors;
    }
}