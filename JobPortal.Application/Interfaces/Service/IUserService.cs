using JobPortal.Application.DTO.Users;

namespace JobPortal.Application.Interfaces.Service;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken ct);
}