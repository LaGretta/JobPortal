using JobPortal.Application.DTO.Auth;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _auth;

    public AccountController(IAuthService auth) => _auth = auth;

    [HttpPost("register/candidate")]
    public async Task<ActionResult<AuthResponseDto>> RegisterCandidate(
        RegisterCandidateDto dto, CancellationToken ct)
        => Ok(await _auth.RegisterCandidateAsync(dto, ct));

    [HttpPost("register/employer")]
    public async Task<ActionResult<AuthResponseDto>> RegisterEmployer(
        RegisterEmployerDto dto, CancellationToken ct)
        => Ok(await _auth.RegisterEmployerAsync(dto, ct));
    

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken ct)
        => Ok(await _auth.LoginAsync(dto, ct));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshDto dto, CancellationToken ct)
        => Ok(await _auth.RefreshAsync(dto, ct));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshDto dto, CancellationToken ct)
    {
        await _auth.LogoutAsync(dto.RefreshToken, ct);
        return NoContent();
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto, CancellationToken ct)
    {
        await _auth.ForgotPasswordAsync(dto, ct);
        // Завжди 200, навіть якщо такого email не існує.
        return Ok(new { message = "Якщо акаунт існує, лист надіслано" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto, CancellationToken ct)
    {
        await _auth.ResetPasswordAsync(dto, ct);
        return Ok(new { message = "Пароль змінено" });
    }
}