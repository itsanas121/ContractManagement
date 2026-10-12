using ContractManagement.Core.Domain.Enums;
using ContractManagement.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Api.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractLifecycleController : ControllerBase
{
    private readonly IContractLifecycleService _lifecycle;

    public ContractLifecycleController(IContractLifecycleService lifecycle)
    {
        _lifecycle = lifecycle;
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Roles = nameof(UserRole.ContractManager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(int id, CancellationToken cancellationToken)
    {
        await _lifecycle.SubmitAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = nameof(UserRole.Reviewer))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        await _lifecycle.ApproveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = nameof(UserRole.Reviewer))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(int id, RejectContractRequest request, CancellationToken cancellationToken)
    {
        await _lifecycle.RejectAsync(id, request.Reason, cancellationToken);
        return NoContent();
    }
}

public record RejectContractRequest(string Reason);