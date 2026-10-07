namespace ContractManagement.Core.Domain.Entities;

public class UserProfile
{
    public int UserId { get; private set; }
    public string? JobTitle { get; private set; }
    public string? Department { get; private set; }
    public string? PhoneNumber { get; private set; }

    private UserProfile() { } // Required by EF Core

    public UserProfile(string? jobTitle, string? department, string? phoneNumber)
    {
        Update(jobTitle, department, phoneNumber);
    }

    public void Update(string? jobTitle, string? department, string? phoneNumber)
    {
        JobTitle = jobTitle;
        Department = department;
        PhoneNumber = phoneNumber;
    }
}