using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Domain.Exceptions;

namespace ContractManagement.Core.Domain.Entities;

public class Company
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public CompanyStatus Status { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public bool IsActive => Status == CompanyStatus.Active;

    private Company() { } 
    
    public Company(string name, string registrationNumber, string? email, string? phone, string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Company name is required.");
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new DomainException("Registration number is required.");
        
        Name = name;
        RegistrationNumber = registrationNumber;
        Email = email;
        Phone = phone;
        Address = address;
        Status = CompanyStatus.Active;
        CreatedDate = DateTime.UtcNow;
    }

    public void Activate() => Status = CompanyStatus.Active;
    public void Deactivate() => Status = CompanyStatus.Inactive;
}