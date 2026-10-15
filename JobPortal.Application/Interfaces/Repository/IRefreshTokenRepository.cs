using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct);

    Task AddAsync(RefreshToken token, CancellationToken ct);
    void Update(RefreshToken token);
    
    // Для logout з усіх пристроїв і при зміні пароля.
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
    Task<int> DeleteExpiredAsync(CancellationToken ct);
}