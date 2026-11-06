using JobPortal.Api.Extensions;
using JobPortal.Application.Common;
using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Profiles;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

[Authorize(Roles = Roles.JobSeeker)]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly ICandidateProfileService _profiles;

    public ProfileController(ICandidateProfileService profiles)
        => _profiles = profiles;

    [HttpGet("my")]
    public async Task<ActionResult<CandidateProfileDto>> GetMy(CancellationToken ct)
        => Ok(await _profiles.GetMyAsync(User.GetUserId(), ct));

    [HttpPut("my")]
    public async Task<ActionResult<CandidateProfileDto>> Update(
        UpdateCandidateProfileDto dto, CancellationToken ct)
        => Ok(await _profiles.UpdateAsync(User.GetUserId(), dto, ct));

    // RequestSizeLimit відсікає великий запит до того, як ASP.NET його
    // забуферить. Без нього 100-мегабайтний файл спершу приїхав би цілком,
    // і тільки потім отримав 400 від нашої перевірки розміру.
    [HttpPost("my/cv")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<CandidateProfileDto>> UploadCv(
        IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Файл не надіслано");

        // await using — потік закриється, навіть якщо сервіс кине виняток.
        await using var stream = file.OpenReadStream();

        var dto = new FileUploadDto
        {
            Content = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length
        };

        return Ok(await _profiles.UploadCvAsync(User.GetUserId(), dto, ct));
    }

    [HttpDelete("my/cv")]
    public async Task<IActionResult> DeleteCv(CancellationToken ct)
    {
        await _profiles.DeleteCvAsync(User.GetUserId(), ct);
        return NoContent();
    }

    [HttpGet("my/cv")]
    public async Task<IActionResult> DownloadCv(CancellationToken ct)
    {
        var f = await _profiles.DownloadMyCvAsync(User.GetUserId(), ct);

        // File() сам закриє потік після відправки.
        return File(f.Content, f.ContentType, f.FileName);
    }
}