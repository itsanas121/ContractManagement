using ContractManagement.Core.Application.DTOs.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    );
}
