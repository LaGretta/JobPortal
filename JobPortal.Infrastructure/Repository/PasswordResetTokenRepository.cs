using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _db;

    public PasswordResetTokenRepository(AppDbContext db) => _db = db;

    public Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken ct)
        => _db.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct)
        => await _db.PasswordResetTokens.AddAsync(token, ct);

    public void Update(PasswordResetToken token)
        => _db.PasswordResetTokens.Update(token);

    //Попросив скидання 3рази - старі листи мають перестати працювати.
    public Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct)
        => _db.PasswordResetTokens
            .Where(x => x.UserId == userId && x.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UsedAt, DateTime.UtcNow), ct);
}