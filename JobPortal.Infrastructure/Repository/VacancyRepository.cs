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
            .Include(x => x.Skills)      // потрібен і для мапінгу, і для заміни навичок в Update
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<(List<Vacancy> Items, int Total)> SearchAsync(
        VacancyFilter filter, CancellationToken ct)
    {
        var query = _db.Vacancies
            .Include(x => x.Company)
            .Include(x => x.Skills)
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

        if (filter.SalaryFrom.HasValue)
            query = query.Where(x => x.SalaryMax >= filter.SalaryFrom);

        if (filter.Category.HasValue)
            query = query.Where(x => x.Category == filter.Category);

        if (filter.ExperienceLevel.HasValue)
            query = query.Where(x => x.ExperienceLevel == filter.ExperienceLevel);

        // Окремий Where на кожну навичку — це І, не АБО.
        // "з C# і SQL" має віддати вакансії, де є обидві, а не будь-яка з них.
        foreach (var raw in filter.Skills ?? [])
        {
            var skill = raw.Trim();
            if (skill.Length == 0)
                continue;

            // Локальна змінна обовʼязкова: EF має підставити значення,
            // а не посилання на змінну цикла.
            query = query.Where(x => x.Skills.Any(s => s.Name == skill));
        }

        int total = await query.CountAsync(ct);

        query = Sort(query, filter);

        var items = await query
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    private static IQueryable<Vacancy> Sort(IQueryable<Vacancy> query, VacancyFilter filter)
        => (filter.SortBy, filter.Desc) switch
        {
            // Найбільша зарплата зверху. Дивимось по максимуму вілки.
            // NULL у SQL Server при спаданні їде в кінець — те, що нам треба.
            (VacancySortBy.Salary, true)
                => query.OrderByDescending(x => x.SalaryMax)
                        .ThenByDescending(x => x.PublishedAt),

            // Найменша зарплата зверху. Тут NULL сам поїхав би на початок,
            // тому спершу сортуємо за "чи є зарплата взагалі".
            (VacancySortBy.Salary, false)
                => query.OrderBy(x => x.SalaryMin == null)
                        .ThenBy(x => x.SalaryMin)
                        .ThenByDescending(x => x.PublishedAt),

            (VacancySortBy.Date, false) => query.OrderBy(x => x.PublishedAt),

            _ => query.OrderByDescending(x => x.PublishedAt)
        };

    public async Task<(List<Vacancy> Items, int Total)> GetByCompanyAsync(Guid companyId, PageRequest page, CancellationToken ct)
    {
        var query = _db.Vacancies
            .Include(x => x.Company)
            .Include(x => x.Skills)      
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