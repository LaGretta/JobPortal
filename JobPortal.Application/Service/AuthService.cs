using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Auth;
using JobPortal.Application.Exceptions;
using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Security;
using JobPortal.Application.Interfaces.Service;
using JobPortal.Domain.Entities;

namespace JobPortal.Application.Service;

public class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly int _accessMinutes;
    private readonly int _refreshDays;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IJwtTokenGenerator jwt,
        ITokenLifetimeProvider lifetimes)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _uow = uow;
        _hasher = hasher;
        _jwt = jwt;
        _accessMinutes = lifetimes.AccessTokenMinutes;
        _refreshDays = lifetimes.RefreshTokenDays;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        string normalized = Normalize(dto.Email);

        if (await _users.ExistsByEmailAsync(normalized, ct))
            throw new ConflictException("Користувач з таким email уже існує");

        var user = new User
        {
            Email = dto.Email.Trim(),
            NormalizedEmail = normalized,
            PasswordHash = _hasher.Hash(dto.Password),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Role = dto.Role
        };

        await _users.AddAsync(user, ct);

        var refresh = CreateRefreshToken(user.Id);
        await _refreshTokens.AddAsync(refresh, ct);

        await _uow.SaveChangesAsync(ct);

        return BuildResponse(user, refresh.Token);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(Normalize(dto.Email), ct);

        // Однакове повідомлення для «нема такого email» і «невірний пароль».
        // Інакше форма логіну стає способом перевірити, хто зареєстрований.
        if (user is null)
            throw new UnauthorizedException("Невірний email або пароль");

        if (user.LockoutUntil is not null && user.LockoutUntil > DateTime.UtcNow)
            throw new UnauthorizedException("Акаунт тимчасово заблоковано. Спробуйте пізніше");

        if (!_hasher.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                user.FailedLoginAttempts = 0;
            }

            _users.Update(user);
            await _uow.SaveChangesAsync(ct);

            throw new UnauthorizedException("Невірний email або пароль");
        }

        // Успіх — лічильник обнуляємо, інакше 4 помилки за тиждень
        // плюс одна сьогодні заблокують людину, яка просто помилялась.
        user.FailedLoginAttempts = 0;
        user.LockoutUntil = null;
        _users.Update(user);

        var refresh = CreateRefreshToken(user.Id);
        await _refreshTokens.AddAsync(refresh, ct);

        await _uow.SaveChangesAsync(ct);

        return BuildResponse(user, refresh.Token);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshDto dto, CancellationToken ct)
    {
        var stored = await _refreshTokens.GetByTokenAsync(dto.RefreshToken, ct);

        if (stored is null)
            throw new UnauthorizedException("Недійсний refresh token");

        // Токен існує, але вже відкликаний → його хтось використав повторно.
        // Або це вкрадений токен, або витік. Вбиваємо всі токени юзера:
        // хай зайде паролем заново.
        if (stored.RevokedAt is not null)
        {
            await _refreshTokens.RevokeAllForUserAsync(stored.UserId, ct);
            throw new UnauthorizedException("Refresh token скомпрометований, увійдіть знову");
        }

        if (stored.IsExpired)
            throw new UnauthorizedException("Refresh token протермінований");

        // Ротація: старий закриваємо, видаємо новий.
        var newToken = CreateRefreshToken(stored.UserId);

        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByToken = newToken.Token;
        _refreshTokens.Update(stored);

        await _refreshTokens.AddAsync(newToken, ct);
        await _uow.SaveChangesAsync(ct);

        return BuildResponse(stored.User, newToken.Token);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        var stored = await _refreshTokens.GetByTokenAsync(refreshToken, ct);

        // Нічого не кидаємо: logout має бути ідемпотентним.
        // Натиснув двічі — обидва рази ок.
        if (stored is null || stored.RevokedAt is not null)
            return;

        stored.RevokedAt = DateTime.UtcNow;
        _refreshTokens.Update(stored);

        await _uow.SaveChangesAsync(ct);
    }

    private static string Normalize(string email)
        => email.Trim().ToUpperInvariant();

    private RefreshToken CreateRefreshToken(Guid userId) => new()
    {
        Token = _jwt.GenerateRefreshToken(),
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(_refreshDays)
    };

    private AuthResponseDto BuildResponse(User user, string refreshToken) => new()
    {
        AccessToken = _jwt.GenerateAccessToken(user),
        RefreshToken = refreshToken,
        AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_accessMinutes),
        User = new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
        }
    };
}