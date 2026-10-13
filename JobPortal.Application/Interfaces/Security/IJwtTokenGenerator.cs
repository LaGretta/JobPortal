using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Security;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}