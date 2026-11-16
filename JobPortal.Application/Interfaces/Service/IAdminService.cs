using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Admin;
using JobPortal.Application.DTO.Vacancies;

namespace JobPortal.Application.Interfaces.Service;

public interface IAdminService
{
    Task<PagedResponse<AdminUserDto>> GetUsersAsync(AdminUserFilter filter, CancellationToken ct);
    Task<PagedResponse<VacancyDto>> GetVacanciesAsync(AdminVacancyFilter filter, CancellationToken ct);
    Task<VacancyDto> CloseVacancyAsync(Guid vacancyId, CancellationToken ct);
}