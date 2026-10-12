using ContractManagement.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.DTOs.Users;

public class UserResponse
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }

    public int CompanyId { get; set; }

    public DateTime CreatedAt { get; set; }
}
