using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Vacancies;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Domain.Enums;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class VacancyRepository : IVacancyRepository
{
    private readonly AppDbContext _db;

    public VacancyRepository(AppDbContext db) => _db = db;

    public Task<Vacancy?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Vacancies
              .Include(x => x.Company)
              .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<(List<Vacancy> Items, int Total)> SearchAsync(
        VacancyFilter filter, CancellationToken ct)
    {
        // AsQueryable + умовні Where: SQL будується з того, що справді прислали.
        // Порожній фільтр не додає нічого в запит.
        var query = _db.Vacancies
            .Include(x => x.Company)
            .Where(x => x.Status == VacancyStatus.Published);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(x => x.Title.Contains(filter.Search));

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(x => x.City == filter.City);

        if (filter.EmploymentType.HasValue)
            query = query.Where(x => x.EmploymentType == filter.EmploymentType);

        if (filter.WorkMode.HasValue)
            query = query.Where(x => x.WorkMode == filter.WorkMode);

        if (filter.Currency.HasValue)
            query = query.Where(x => x.Currency == filter.Currency);

        // "Від X" дивимось по максимуму вілки: вакансія 4000–8000
        // підходить під запит "від 6000".
        if (filter.SalaryFrom.HasValue)
            query = query.Where(x => x.SalaryMax >= filter.SalaryFrom);

        // Рахуємо ДО Skip/Take — інакше отримаємо кількість на сторінці,
        // а не загальну.
        int total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.PublishedAt)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(List<Vacancy> Items, int Total)> GetByCompanyAsync(Guid companyId, PageRequest page, CancellationToken ct)
    {
        var query = _db.Vacancies
            .Include(x => x.Company)
            .Where(x => x.CompanyId == companyId);

        int total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

  
    public async Task AddAsync(Vacancy vacancy, CancellationToken ct)
        => await _db.Vacancies.AddAsync(vacancy, ct);

    public void Update(Vacancy vacancy) => _db.Vacancies.Update(vacancy);
}