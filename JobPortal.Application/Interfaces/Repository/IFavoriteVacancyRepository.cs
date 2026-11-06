using JobPortal.Application.DTO;
using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface IFavoriteVacancyRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid vacancyId, CancellationToken ct);
    
    Task<(List<FavoriteVacancy> Items, int Total)> GetByUserAsync(
        Guid userId, PageRequest page, CancellationToken ct);

    Task AddAsync(FavoriteVacancy favorite, CancellationToken ct);

    Task<int> RemoveAsync(Guid userId, Guid vacancyId, CancellationToken ct);
}