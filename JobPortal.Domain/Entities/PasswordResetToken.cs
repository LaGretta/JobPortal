namespace JobPortal.Domain.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // ХЕШ токена, не сам токен. Юзеру в лист іде відкритий рядок,
    // у базі лежить його хеш. Витече база — токени непридатні.
    public string TokenHash { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; set; }

    public bool IsActive => UsedAt == null && DateTime.UtcNow < ExpiresAt;
}