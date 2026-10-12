namespace JobPortal.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Коли відкликали. Null — токен живий.
    // Саме це поле і є logout: JWT відкликати неможливо, а рядок у базі — можна.
    public DateTime? RevokedAt { get; set; }

    // На що замінили при ротації. Потрібно, щоб зловити крадіжку:
    // якщо прийшов уже використаний токен — хтось його вкрав,
    // і тоді вбиваємо весь ланцюжок.
    public string? ReplacedByToken { get; set; }

    // Обчислювані властивості: не поля, а відповіді на питання.
    // [NotMapped] не потрібен — EF бачить, що сетера нема, і в базу їх не кладе.
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;
}