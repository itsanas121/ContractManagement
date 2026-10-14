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

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ContractSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContractSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        return Ok(await _contracts.GetSummaryAsync(cancellationToken));
    }

    [HttpGet("{id:int}/audit")]
    [ProducesResponseType(typeof(IReadOnlyList<ContractAuditEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ContractAuditEntryResponse>>> GetAuditHistory(
    int id, CancellationToken cancellationToken)
    {
        return Ok(await _contracts.GetAuditHistoryAsync(id, cancellationToken));
    }

    [HttpPost("{id:int}/parties")]
    [Authorize(Roles = nameof(UserRole.ContractManager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddParty(
        int id, AddContractPartyRequest request, CancellationToken cancellationToken)
    {
        await _contracts.AddPartyAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/documents")]
    [Authorize(Roles = nameof(UserRole.ContractManager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddDocument(
        int id, AddContractDocumentRequest request, CancellationToken cancellationToken)
    {
        await _contracts.AddDocumentAsync(id, request, cancellationToken);
        return NoContent();
    }
}