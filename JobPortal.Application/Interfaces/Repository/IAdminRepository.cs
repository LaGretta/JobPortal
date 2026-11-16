using JobPortal.Application.DTO.Admin;
using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface IAdminRepository
{
    Task<(List<User> Items, int Total)> GetUsersAsync(AdminUserFilter filter, CancellationToken ct);
    Task<(List<Vacancy> Items, int Total)> GetVacanciesAsync(AdminVacancyFilter filter, CancellationToken ct);
}