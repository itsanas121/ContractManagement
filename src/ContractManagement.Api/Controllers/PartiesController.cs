using ContractManagement.Core.Application.DTOs.Parties;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Api.Controllers;

[ApiController]
[Route("api/parties")]
[Authorize]
public class PartiesController : ControllerBase
{
    private const string WriteRoles =
        nameof(UserRole.Administrator) + "," + nameof(UserRole.ContractManager);
    private const string ReadRoles = WriteRoles; // widen here later

    private readonly IPartyService _parties;

    public PartiesController(IPartyService parties)
    {
        _parties = parties;
    }

    [HttpPost("individual")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(typeof(PartyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PartyResponse>> RegisterIndividual(
        CreateIndividualPartyRequest request, CancellationToken cancellationToken)
    {
        var response = await _parties.RegisterIndividualAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("organization")]
    [Authorize(Roles = WriteRoles)]
    [ProducesResponseType(typeof(PartyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PartyResponse>> RegisterOrganization(
        CreateOrganizationPartyRequest request, CancellationToken cancellationToken)
    {
        var response = await _parties.RegisterOrganizationAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    [Authorize(Roles = ReadRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<PartyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<PartyResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _parties.GetAllAsync(cancellationToken));
    }
}