using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Users
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, ct);

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct)
        => _db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, ct);

    public async Task AddAsync(User user, CancellationToken ct)
        => await _db.Users.AddAsync(user, ct);

    public void Update(User user) => _db.Users.Update(user);
}