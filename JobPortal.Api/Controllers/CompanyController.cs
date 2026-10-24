using JobPortal.Api.Extensions;
using JobPortal.Application.Common;
using JobPortal.Application.DTO.Companies;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companies;

    public CompanyController(ICompanyService companies) => _companies = companies;

    // Публічний: здобувач дивиться компанію у вакансії.
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _companies.GetByIdAsync(id, ct));

    
    [Authorize(Roles = Roles.Employer)]
    [HttpGet("my")]
    public async Task<ActionResult<CompanyDto>> GetMy(CancellationToken ct)
        => Ok(await _companies.GetMyAsync(User.GetUserId(), ct));

    [Authorize(Roles = Roles.Employer)]
    [HttpPut("my")]
    public async Task<ActionResult<CompanyDto>> UpdateMy(
        UpdateCompanyDto dto, CancellationToken ct)
        => Ok(await _companies.UpdateMyAsync(User.GetUserId(), dto, ct));
}