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

public class FavoriteVacancyService : IFavoriteVacancyService
{
    private readonly IFavoriteVacancyRepository _favorites;
    private readonly IVacancyRepository _vacancies;
    private readonly IUnitOfWork _uow;

    public FavoriteVacancyService(
        IFavoriteVacancyRepository favorites,
        IVacancyRepository vacancies,
        IUnitOfWork uow)
    {
        _favorites = favorites;
        _vacancies = vacancies;
        _uow = uow;
    }

    public async Task AddAsync(Guid userId, Guid vacancyId, CancellationToken ct)
    {
        var vacancy = await _vacancies.GetByIdAsync(vacancyId, ct);

        // Та сама логіка, що у відгуку: чернетка й закрита — наче їх нема.
        if (vacancy is null || vacancy.Status != VacancyStatus.Published)
            throw new NotFoundException("Вакансію не знайдено");

        // Дружня 409 замість помилки унікального індексу.
        if (await _favorites.ExistsAsync(userId, vacancyId, ct))
            throw new ConflictException("Вакансія вже в обраному");

        await _favorites.AddAsync(new FavoriteVacancy
        {
            UserId = userId,
            VacancyId = vacancyId
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Guid userId, Guid vacancyId, CancellationToken ct)
    {
        int removed = await _favorites.RemoveAsync(userId, vacancyId, ct);

        // 0 рядків — або чужа вакансія, або її не було в обраному.
        // Для клієнта різниці нема: у його обраному такого нема.
        if (removed == 0)
            throw new NotFoundException("Вакансії немає в обраному");
    }

    public async Task<PagedResponse<VacancyDto>> GetMyAsync(
        Guid userId, PageRequest page, CancellationToken ct)
    {
        var (items, total) = await _favorites.GetByUserAsync(userId, page, ct);

        return new PagedResponse<VacancyDto>
        {
            Items = items.Select(x => VacancyMapper.ToDto(x.Vacancy)).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = total
        };
    }
}