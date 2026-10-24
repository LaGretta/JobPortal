using JobPortal.Application.DTO.Users;
using JobPortal.Domain.Enums;

namespace JobPortal.Application.DTO.Auth;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public UserSummaryDto User { get; set; } = null!;
}
public class RefreshDto
{
    public string RefreshToken { get; set; } = string.Empty;
}