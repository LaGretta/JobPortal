namespace JobPortal.Domain.Entities;

public class Company
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    // Власник — той роботодавець, що зареєстрував компанію.
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}