using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db) => _db = db;

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct)
        => _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct)
        => await _db.RefreshTokens.AddAsync(token, ct);

    public void Update(RefreshToken token) => _db.RefreshTokens.Update(token);

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
        => _db.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, DateTime.UtcNow), ct);

    public Task<int> DeleteExpiredAsync(CancellationToken ct)
        => _db.RefreshTokens
            .Where(x => x.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);
}