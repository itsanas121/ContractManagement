using ContractManagement.Api.Domain.Enums;
using ContractManagement.Api.Domain.Exceptions;

namespace ContractManagement.Api.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string FirstName { get; private set; } = String.Empty;
    public string LastName { get; private set; } = String.Empty;
    public string Email { get; private set; } = String.Empty;
    public string EmployeeNumber { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public int CompanyId { get; private set; }
    public Company Company { get; private set; } = null!;
    public UserProfile? Profile { get; private set; }

    public string FullName => $"{FirstName} {LastName}";
    public bool IsActive => Status == UserStatus.Active;

    private User() { }

    public User(string firstName, string lastName, string email, string employeeNumber, Company company)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new DomainException("Employee number is required.");
        if (!company.IsActive)
            throw new DomainException("A user cannot be assigned to an inactive company.");

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        EmployeeNumber = employeeNumber;
        Company = company;
        Status = UserStatus.Active;
        CreatedDate = DateTime.UtcNow;
    }

    public void Deactivate() => Status = UserStatus.Inactive;

    public void SetProfile(string? jobTitle, string? department, string? phoneNumber)
    {
        if (Profile is null)
            Profile = new UserProfile(jobTitle, department, phoneNumber);
        else
            Profile.Update(jobTitle, department, phoneNumber);
    }

    public void Activate()
    {
        if (!Company.IsActive)
            throw new DomainException("Cannot activate a user whose company is inactive.");
        else
            Status = UserStatus.Active;
    }
}