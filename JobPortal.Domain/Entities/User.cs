using JobPortal.Domain.Enums;

namespace JobPortal.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.JobSeeker;
    public bool EmailConfirmed { get; set; }

    // Проти перебору пароля: рахуємо невдалі спроби й тимчасово блокуємо.
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutUntil { get; set; }

    // UtcNow, не Now. Сервер в Azure в іншому поясі, ніж ти.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RefreshToken> RefreshTokens { get; set; } = new();
}