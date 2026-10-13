using ContractManagement.Core.Application.DTOs.Companies;
using ContractManagement.Core.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.Services;

public interface IUserService
{
    Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<List<UserResponse>> GetAllAsync(
    CancellationToken cancellationToken = default);
}
