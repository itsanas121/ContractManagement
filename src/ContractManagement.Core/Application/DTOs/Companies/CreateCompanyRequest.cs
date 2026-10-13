using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.DTOs.Companies;

public class CreateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}
