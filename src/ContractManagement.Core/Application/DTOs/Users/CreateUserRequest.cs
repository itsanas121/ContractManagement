using ContractManagement.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.DTOs.Users;

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public int CompanyId { get; set; }

}
