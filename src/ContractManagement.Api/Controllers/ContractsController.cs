using ContractManagement.Core.Application.DTOs.Contracts;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contracts;

    public ContractsController(IContractService contracts)
    {
        _contracts = contracts;
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.ContractManager))]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> Create(
        CreateContractRequest request, CancellationToken cancellationToken)
    {
        var response = await _contracts.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}