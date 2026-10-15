using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    void Update(User user);
}