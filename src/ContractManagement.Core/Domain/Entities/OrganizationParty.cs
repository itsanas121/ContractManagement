namespace ContractManagement.Core.Domain.Entities;

public class OrganizationParty : Party
{
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string ContactPerson { get; private set; } = string.Empty;

    private OrganizationParty() { } // Required by EF Core

    public OrganizationParty(string name, string registrationNumber,
        string contactPerson, string email, string phone)
        : base(name, email, phone)
    {
        RegistrationNumber = registrationNumber;
        ContactPerson = contactPerson;
        EnsureValid();
    }

    public override List<string> Validate()
    {
        var errors = base.Validate();

        if (RegistrationNumber.Length != 10 || !RegistrationNumber.All(char.IsDigit))
            errors.Add("Registration number must be exactly 10 digits.");
        if (string.IsNullOrWhiteSpace(ContactPerson))
            errors.Add("Contact person is required.");

        return errors;
    }
}