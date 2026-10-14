using ContractManagement.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.DTOs.Users;

public class CreateUserRequest
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string EmployeeNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public UserRole? Role { get; set; }
    public int CompanyId { get; set; }

    public UserProfileDto? Profile { get; set; }

}
