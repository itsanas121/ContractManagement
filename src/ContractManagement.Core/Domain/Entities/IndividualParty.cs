namespace ContractManagement.Core.Domain.Entities;

public class IndividualParty : Party
{
    public string Name { get; private set; } = string.Empty;
    public string NationalId { get; private set; } = string.Empty;

    private IndividualParty() { } // Required by EF Core

    public IndividualParty(string name, string nationalId, string email, string phone)
        : base(email, phone)
    {
        Name = name;
        NationalId = nationalId;
        EnsureValid();
    }

    public override string DisplayName => Name;

    public override List<string> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Name is required.");
        if (NationalId.Length != 10 || !NationalId.All(char.IsDigit))
            errors.Add("National ID must be exactly 10 digits.");

        return errors;
    }
}