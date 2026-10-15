using JobPortal.Application.DTO;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAuthService _auth;

    public AccountController(IAuthService auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto, CancellationToken ct)
        => Ok(await _auth.RegisterAsync(dto, ct));

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
}