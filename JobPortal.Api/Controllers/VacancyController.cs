using JobPortal.Api.Extensions;
using JobPortal.Application.Common;
using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Vacancies;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VacancyController : ControllerBase
{
    private readonly IVacancyService _vacancies;

    public VacancyController(IVacancyService vacancies) => _vacancies = vacancies;

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<VacancyDto>>> Search(
        [FromQuery] VacancyFilter filter, CancellationToken ct)
        => Ok(await _vacancies.SearchAsync(filter, ct));

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VacancyDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _vacancies.GetByIdAsync(id, ct));

    [Authorize(Roles = Roles.Employer)]
    [HttpGet("my")]
    public async Task<ActionResult<PagedResponse<VacancyDto>>> GetMy(
        [FromQuery] PageRequest page, CancellationToken ct)
        => Ok(await _vacancies.GetMyAsync(User.GetUserId(), page, ct));

    [Authorize(Roles = Roles.Employer)]
    [HttpPost]
    public async Task<ActionResult<VacancyDto>> Create(
        CreateVacancyDto dto, CancellationToken ct)
        => Ok(await _vacancies.CreateAsync(User.GetUserId(), dto, ct));

    [Authorize(Roles = Roles.Employer)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VacancyDto>> Update(
        Guid id, UpdateVacancyDto dto, CancellationToken ct)
        => Ok(await _vacancies.UpdateAsync(User.GetUserId(), id, dto, ct));

    [Authorize(Roles = Roles.Employer)]
    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<VacancyDto>> Publish(Guid id, CancellationToken ct)
        => Ok(await _vacancies.PublishAsync(User.GetUserId(), id, ct));

    [Authorize(Roles = Roles.Employer)]
    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<VacancyDto>> Close(Guid id, CancellationToken ct)
        => Ok(await _vacancies.CloseAsync(User.GetUserId(), id, ct));
    
}