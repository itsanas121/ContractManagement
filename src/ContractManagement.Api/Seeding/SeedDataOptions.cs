namespace ContractManagement.Api.Seeding;

public class SeedDataOptions
{
    public string Password { get; set; } = string.Empty;

    public List<SeedCompanyOptions> Companies { get; set; } = new();
    public List<SeedUserOptions> Users { get; set; } = new();
}

public class SeedCompanyOptions
{
    public string Name { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }
}

public class SeedUserOptions
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string CompanyRegistrationNumber { get; set; } = string.Empty;
}
