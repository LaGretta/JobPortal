using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface ICompanyRepository
{
    Task<Company?> GetByOwnerAsync(Guid ownerId, CancellationToken ct);
    Task<Company?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Company company, CancellationToken ct);
    void Update(Company company);
}