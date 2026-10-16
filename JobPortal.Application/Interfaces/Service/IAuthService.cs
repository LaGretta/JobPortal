using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Auth;

namespace JobPortal.Application.Interfaces.Service;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct);
    Task<AuthResponseDto> RefreshAsync(RefreshDto dto, CancellationToken ct);
    Task LogoutAsync(string refreshToken, CancellationToken ct);
    
    Task ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct);
    Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct);
}