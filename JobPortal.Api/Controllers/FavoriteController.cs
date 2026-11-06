using JobPortal.Api.Extensions;
using JobPortal.Application.Common;
using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Vacancies;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

// Обране є тільки в того, хто шукає роботу.
[Authorize(Roles = Roles.JobSeeker)]
[ApiController]
[Route("api/[controller]")]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteVacancyService _favorites;

    public FavoriteController(IFavoriteVacancyService favorites)
        => _favorites = favorites;

    [HttpPost("{vacancyId:guid}")]
    public async Task<IActionResult> Add(Guid vacancyId, CancellationToken ct)
    {
        await _favorites.AddAsync(User.GetUserId(), vacancyId, ct);
        return NoContent();   // 204: тіло не потрібне, фронт і так знає, що додав
    }

    [HttpDelete("{vacancyId:guid}")]
    public async Task<IActionResult> Remove(Guid vacancyId, CancellationToken ct)
    {
        await _favorites.RemoveAsync(User.GetUserId(), vacancyId, ct);
        return NoContent();
    }

    [HttpGet("my")]
    public async Task<ActionResult<PagedResponse<VacancyDto>>> GetMy(
        [FromQuery] PageRequest page, CancellationToken ct)
        => Ok(await _favorites.GetMyAsync(User.GetUserId(), page, ct));
}