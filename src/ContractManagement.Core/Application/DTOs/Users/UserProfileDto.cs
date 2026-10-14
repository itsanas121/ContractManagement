using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.DTOs.Users;

public class UserProfileDto
{
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? PhoneNumber { get; set; }
}
