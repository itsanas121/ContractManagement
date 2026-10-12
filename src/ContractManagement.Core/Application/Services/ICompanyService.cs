using ContractManagement.Core.Application.DTOs.Companies;
using ContractManagement.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractManagement.Core.Application.Services;

public interface ICompanyService
{
    Task<CompanyResponse> CreateAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default);
}
