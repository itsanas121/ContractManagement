using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Domain.Entities;

public abstract class Party
{
    public int Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }

    protected Party() { } // Required by EF Core

    protected Party(string email, string phone)
    {
        Email = email;
        Phone = phone;
        CreatedDate = DateTime.UtcNow;
    }

    public abstract string DisplayName { get; }

    public virtual List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@'))
            errors.Add("A valid email is required.");
        if (string.IsNullOrWhiteSpace(Phone))
            errors.Add("Phone is required.");

        return errors;
    }

    protected void EnsureValid()
    {
        var errors = Validate();
        if (errors.Count > 0)
            throw new DomainValidationException(errors);
    }
}