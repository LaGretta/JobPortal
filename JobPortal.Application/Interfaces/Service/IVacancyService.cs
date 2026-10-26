using JobPortal.Application.DTO.Vacancies;
using JobPortal.Application.DTO;

namespace JobPortal.Application.Interfaces.Service;

public interface IVacancyService
{
    Task<PagedResponse<VacancyDto>> SearchAsync(VacancyFilter filter, CancellationToken ct);
    Task<VacancyDto> GetByIdAsync(Guid id, CancellationToken ct);

    Task<PagedResponse<VacancyDto>> GetMyAsync(Guid ownerId, PageRequest page, CancellationToken ct);
    Task<VacancyDto> CreateAsync(Guid ownerId, CreateVacancyDto dto, CancellationToken ct);
    Task<VacancyDto> UpdateAsync(Guid ownerId, Guid id, UpdateVacancyDto dto, CancellationToken ct);
    Task<VacancyDto> PublishAsync(Guid ownerId, Guid id, CancellationToken ct);
    Task<VacancyDto> CloseAsync(Guid ownerId, Guid id, CancellationToken ct);
}