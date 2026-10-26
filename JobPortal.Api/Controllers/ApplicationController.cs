using JobPortal.Api.Extensions;
using JobPortal.Application.Common;
using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Applications;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IJobApplicationService _applications;

    public ApplicationController(IJobApplicationService applications)
        => _applications = applications;

    [Authorize(Roles = Roles.JobSeeker)]
    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> Apply(
        CreateApplicationDto dto, CancellationToken ct)
        => Ok(await _applications.ApplyAsync(User.GetUserId(), dto, ct));

    [Authorize(Roles = Roles.JobSeeker)]
    [HttpGet("my")]
    public async Task<ActionResult<PagedResponse<ApplicationDto>>> GetMy(
        [FromQuery] ApplicationFilter filter, CancellationToken ct)
        => Ok(await _applications.GetMyAsync(User.GetUserId(), filter, ct));

    [Authorize(Roles = Roles.Employer)]
    [HttpGet("vacancy/{vacancyId:guid}")]
    public async Task<ActionResult<PagedResponse<ApplicationDto>>> GetByVacancy(
        Guid vacancyId, [FromQuery] ApplicationFilter filter, CancellationToken ct)
        => Ok(await _applications.GetByVacancyAsync(User.GetUserId(), vacancyId, filter, ct));

    [Authorize(Roles = Roles.Employer)]
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApplicationDto>> UpdateStatus(
        Guid id, UpdateApplicationStatusDto dto, CancellationToken ct)
        => Ok(await _applications.UpdateStatusAsync(User.GetUserId(), id, dto, ct));
}