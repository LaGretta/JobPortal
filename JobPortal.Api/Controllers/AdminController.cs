using JobPortal.Application.Common;
using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Admin;
using JobPortal.Application.DTO.Vacancies;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

[Authorize(Policy = Policies.Admin)]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    [HttpGet("users")]
    public async Task<ActionResult<PagedResponse<AdminUserDto>>> GetUsers(
        [FromQuery] AdminUserFilter filter, CancellationToken ct)
        => Ok(await _admin.GetUsersAsync(filter, ct));

    [HttpGet("vacancies")]
    public async Task<ActionResult<PagedResponse<VacancyDto>>> GetVacancies(
        [FromQuery] AdminVacancyFilter filter, CancellationToken ct)
        => Ok(await _admin.GetVacanciesAsync(filter, ct));

    [HttpPost("vacancies/{id:guid}/close")]
    public async Task<ActionResult<VacancyDto>> CloseVacancy(Guid id, CancellationToken ct)
        => Ok(await _admin.CloseVacancyAsync(id, ct));
}