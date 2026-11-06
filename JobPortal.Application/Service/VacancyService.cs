using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Vacancies;
using JobPortal.Application.Exceptions;
using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Service;
using JobPortal.Application.Mapping;
using JobPortal.Domain.Entities;
using JobPortal.Domain.Enums;

namespace JobPortal.Application.Service;

public class VacancyService : IVacancyService
{
    private readonly IVacancyRepository _vacancies;
    private readonly ICompanyRepository _companies;
    private readonly IUnitOfWork _uow;
    

    public VacancyService(
        IVacancyRepository vacancies,
        ICompanyRepository companies,
        IUnitOfWork uow)
    {
        _vacancies = vacancies;
        _companies = companies;
        _uow = uow;
    }

    public async Task<PagedResponse<VacancyDto>> SearchAsync(
        VacancyFilter filter, CancellationToken ct)
    {
        var (items, total) = await _vacancies.SearchAsync(filter, ct);

        return new PagedResponse<VacancyDto>
        {
            Items = items.Select(Map).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task<VacancyDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var vacancy = await _vacancies.GetByIdAsync(id, ct);

        // Чернетку чи закриту вакансію публічно не віддаємо —
        // інакше посилання на непублічну вакансію працюватиме для всіх.
        if (vacancy is null || vacancy.Status != VacancyStatus.Published)
            throw new NotFoundException("Вакансію не знайдено");

        return Map(vacancy);
    }

    public async Task<PagedResponse<VacancyDto>> GetMyAsync(
        Guid ownerId, PageRequest page, CancellationToken ct)
    {
        var company = await GetMyCompanyAsync(ownerId, ct);
        var (items, total) = await _vacancies.GetByCompanyAsync(company.Id, page, ct);

        return new PagedResponse<VacancyDto>
        {
            Items = items.Select(Map).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = total
        };
    }

    public async Task<VacancyDto> CreateAsync(
        Guid ownerId, CreateVacancyDto dto, CancellationToken ct)
    {
        var company = await GetMyCompanyAsync(ownerId, ct);

        var vacancy = new Vacancy
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            City = dto.City.Trim(),
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            Currency = dto.Currency,
            EmploymentType = dto.EmploymentType,
            WorkMode = dto.WorkMode,
            Category = dto.Category,
            ExperienceLevel = dto.ExperienceLevel,
            Skills = BuildSkills(dto.Skills),
            CompanyId = company.Id,
            Status = VacancyStatus.Draft
        };

        await _vacancies.AddAsync(vacancy, ct);
        await _uow.SaveChangesAsync(ct);

        // Company не завантажена після Add — підставляємо для відповіді.
        vacancy.Company = company;
        return Map(vacancy);
    }

    public async Task<VacancyDto> UpdateAsync(
        Guid ownerId, Guid id, UpdateVacancyDto dto, CancellationToken ct)
    {
        var vacancy = await GetOwnedVacancyAsync(ownerId, id, ct);

        vacancy.Title = dto.Title.Trim();
        vacancy.Description = dto.Description.Trim();
        vacancy.City = dto.City.Trim();
        vacancy.SalaryMin = dto.SalaryMin;
        vacancy.SalaryMax = dto.SalaryMax;
        vacancy.Currency = dto.Currency;
        vacancy.EmploymentType = dto.EmploymentType;
        vacancy.WorkMode = dto.WorkMode;
        vacancy.Category = dto.Category;
        vacancy.ExperienceLevel = dto.ExperienceLevel;

        // Навички замінюємо цілком, а не доповнюємо: клієнт прислав
        // повний список, значить чого в ньому нема — те видалено.
        vacancy.Skills.Clear();
        vacancy.Skills.AddRange(BuildSkills(dto.Skills));

        _vacancies.Update(vacancy);
        await _uow.SaveChangesAsync(ct);

        return Map(vacancy);
    }

    public async Task<VacancyDto> PublishAsync(Guid ownerId, Guid id, CancellationToken ct)
    {
        var vacancy = await GetOwnedVacancyAsync(ownerId, id, ct);

        vacancy.Status = VacancyStatus.Published;

        // Ставимо дату тільки при першій публікації, щоб вакансія
        // не стрибала на початок списку після кожного редагування.
        vacancy.PublishedAt ??= DateTime.UtcNow;

        _vacancies.Update(vacancy);
        await _uow.SaveChangesAsync(ct);

        return Map(vacancy);
    }

    public async Task<VacancyDto> CloseAsync(Guid ownerId, Guid id, CancellationToken ct)
    {
        var vacancy = await GetOwnedVacancyAsync(ownerId, id, ct);

        vacancy.Status = VacancyStatus.Closed;

        _vacancies.Update(vacancy);
        await _uow.SaveChangesAsync(ct);

        return Map(vacancy);
    }
    
    
    

    private async Task<Company> GetMyCompanyAsync(Guid ownerId, CancellationToken ct)
    {
        var company = await _companies.GetByOwnerAsync(ownerId, ct);

        if (company is null)
            throw new NotFoundException("Компанію не знайдено");

        return company;
    }

    // Ось тут перевірка "це моє".
    private async Task<Vacancy> GetOwnedVacancyAsync(
        Guid ownerId, Guid id, CancellationToken ct)
    {
        var vacancy = await _vacancies.GetByIdAsync(id, ct);

        if (vacancy is null)
            throw new NotFoundException("Вакансію не знайдено");

        var company = await GetMyCompanyAsync(ownerId, ct);

        // 403, не 404: вакансія існує, але вона чужа.
        if (vacancy.CompanyId != company.Id)
            throw new ForbiddenException("Це не ваша вакансія");

        return vacancy;
    }
    private static VacancyDto Map(Vacancy v) => VacancyMapper.ToDto(v);
        private static List<VacancySkill> BuildSkills(List<string>? names)
        => (names ?? [])
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)                     // пусті рядки відкидаємо
            .DistinctBy(x => x.ToLowerInvariant())        // "C#" і "c#" — одна навичка
            .Take(20)                                     // стеля, щоб не прислали тисячу
            .Select(x => new VacancySkill { Name = x })
            .ToList();
} 