using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Admin;
using JobPortal.Application.DTO.Vacancies;
using JobPortal.Application.Exceptions;
using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Service;
using JobPortal.Application.Mapping;
using JobPortal.Domain.Entities;
using JobPortal.Domain.Enums;

namespace JobPortal.Application.Service;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _admin;
    private readonly IVacancyRepository _vacancies;
    private readonly IUnitOfWork _uow;

    public AdminService(
        IAdminRepository admin,
        IVacancyRepository vacancies,
        IUnitOfWork uow)
    {
        _admin = admin;
        _vacancies = vacancies;
        _uow = uow;
    }

    public async Task<PagedResponse<AdminUserDto>> GetUsersAsync(
        AdminUserFilter filter, CancellationToken ct)
    {
        var (items, total) = await _admin.GetUsersAsync(filter, ct);

        return new PagedResponse<AdminUserDto>
        {
            Items = items.Select(Map).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task<PagedResponse<VacancyDto>> GetVacanciesAsync(
        AdminVacancyFilter filter, CancellationToken ct)
    {
        var (items, total) = await _admin.GetVacanciesAsync(filter, ct);

        return new PagedResponse<VacancyDto>
        {

            Items = items.Select(VacancyMapper.ToDto).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task<VacancyDto> CloseVacancyAsync(Guid vacancyId, CancellationToken ct)
    {
        var vacancy = await _vacancies.GetByIdAsync(vacancyId, ct);

        if (vacancy is null)
            throw new NotFoundException("Вакансію не знайдено");
        
        if (vacancy.Status == VacancyStatus.Closed)
            return VacancyMapper.ToDto(vacancy);

        vacancy.Status = VacancyStatus.Closed;

        _vacancies.Update(vacancy);
        await _uow.SaveChangesAsync(ct);

        return VacancyMapper.ToDto(vacancy);
    }

    private static AdminUserDto Map(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        PhoneNumber = u.PhoneNumber,
        Role = u.Role,
        EmailConfirmed = u.EmailConfirmed,
        CreatedAt = u.CreatedAt,
        CompanyName = u.Company?.Name,

        IsLockedOut = u.LockoutUntil.HasValue && u.LockoutUntil > DateTime.UtcNow
    };
}