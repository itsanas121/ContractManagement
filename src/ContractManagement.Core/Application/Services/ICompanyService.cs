using ContractManagement.Core.Application.DTOs.Companies;
using ContractManagement.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ContractManagement.Core.Application.Services;

public interface ICompanyService
{
    Task<CompanyResponse> CreateAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default);

    Task<List<CompanyResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task ActivateAsync(
        int companyId,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        int companyId,
        CancellationToken cancellationToken = default);
}
