using JobPortal.Application.Interfaces.Security;
using JobPortal.Domain.Entities;
using JobPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobPortal.Infrastructure.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct)
    {
        var settings = services.GetRequiredService<IOptions<AdminSettings>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");
        if (string.IsNullOrWhiteSpace(settings.Email) || string.IsNullOrWhiteSpace(settings.Password))
        {
            logger.LogInformation("Admin:Email або Admin:Password не задані — адміна не створюю");
            return;
        }

        var db = services.GetRequiredService<AppDbContext>();
        if (!await db.Database.CanConnectAsync(ct))
        {
            logger.LogWarning("База недоступна — засіяти адміна не можу");
            return;
        }
        var normalized = settings.Email.Trim().ToUpperInvariant();

        var existing = await db.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalized, ct);

        if (existing is not null)
        {
            if (existing.Role != UserRole.Admin)
                logger.LogWarning("Користувач {Email} існує з роллю {Role} — адміном не робитиму",
                    existing.Email, existing.Role);

            return;
        }

        var hasher = services.GetRequiredService<IPasswordHasher>();

        db.Users.Add(new User
        {
            Email = settings.Email.Trim(),
            NormalizedEmail = normalized,
            PasswordHash = hasher.Hash(settings.Password),
            FirstName = string.IsNullOrWhiteSpace(settings.FirstName) ? "Admin" : settings.FirstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(settings.LastName) ? "Admin" : settings.LastName.Trim(),
            Role = UserRole.Admin,
            EmailConfirmed = true
        });

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Створено адміна {Email}", settings.Email);
    }
}