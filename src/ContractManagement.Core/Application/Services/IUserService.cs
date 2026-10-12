using ContractManagement.Core.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        )
}
