using JobPortal.Api.Extensions;
using JobPortal.Application.DTO.Users;
using JobPortal.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IUserService _users;

    public AccountController(IUserService users) => _users = users;

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken ct)
        => Ok(await _users.GetProfileAsync(User.GetUserId(), ct));
    
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe(
        UpdateProfileDto dto, CancellationToken ct)
        => Ok(await _users.UpdateProfileAsync(User.GetUserId(), dto, ct));
}