using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken ct);
    Task AddAsync(PasswordResetToken token, CancellationToken ct);
    void Update(PasswordResetToken token);
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct);
}