using JobPortal.Application.DTO.Users;
using JobPortal.Application.Exceptions;
using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Service;

namespace JobPortal.Application.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
         IUserRepository users
        ,IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(userId, ct);

        if (user is null)
            throw new NotFoundException("Користувача не знайдено");

        return new UserProfileDto
        {
            Id = user.Id,
            CompanyId = user.Company?.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(
        Guid userId, UpdateProfileDto dto, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(userId, ct);

        if (user is null)
            throw new NotFoundException("Користувача не знайдено");

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);
        
        //оновлений профіль - щоб фронт не робив другий GET.
        return await GetProfileAsync(userId, ct);
    }
}