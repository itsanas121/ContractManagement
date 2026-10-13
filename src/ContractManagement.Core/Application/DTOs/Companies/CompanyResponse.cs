using ContractManagement.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.DTOs.Companies;

public class CompanyResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public CompanyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
