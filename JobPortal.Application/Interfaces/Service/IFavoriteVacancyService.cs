using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Vacancies;

namespace JobPortal.Application.Interfaces.Service;

public interface IFavoriteVacancyService
{
    Task AddAsync(Guid userId, Guid vacancyId, CancellationToken ct);
    Task RemoveAsync(Guid userId, Guid vacancyId, CancellationToken ct);
    Task<PagedResponse<VacancyDto>> GetMyAsync(Guid userId, PageRequest page, CancellationToken ct);
}