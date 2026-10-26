using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Vacancies;
using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface IVacancyRepository
{
    Task<Vacancy?> GetByIdAsync(Guid id, CancellationToken ct);

    // Публічний пошук — тільки опубліковані.
    Task<(List<Vacancy> Items, int Total)> SearchAsync(VacancyFilter filter, CancellationToken ct);

    // Вакансії роботодавця — включно з чернетками й закритими.
    Task<(List<Vacancy> Items, int Total)> GetByCompanyAsync(
        Guid companyId, PageRequest page, CancellationToken ct);

    Task AddAsync(Vacancy vacancy, CancellationToken ct);
    void Update(Vacancy vacancy);
}