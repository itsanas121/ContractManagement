using ContractManagement.Core.Application.DTOs.Companies;
using ContractManagement.Core.Application.Services;
using ContractManagement.Core.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ContractManagement.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize(Roles = nameof(UserRole.Administrator))]
public class CompanyController: ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
            {
        _companyService = companyService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CompanyResponse>> Create(
        CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _companyService.CreateAsync(
            request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CompanyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CompanyResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var companies = await _companyService.GetAllAsync(
            cancellationToken);
        return Ok(companies);
    }

    [HttpPost("{id:int}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Activate(
    int id,
    CancellationToken cancellationToken)
    {
        await _companyService.ActivateAsync(
            id, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:int}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(
    int id,
    CancellationToken cancellationToken)
    {
        await _companyService.DeactivateAsync(
            id, cancellationToken);

        return NoContent();
    }
}
