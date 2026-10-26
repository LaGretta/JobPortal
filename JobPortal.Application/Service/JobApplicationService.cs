using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Applications;
using JobPortal.Application.Exceptions;
using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Infrastructure;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Service;
using JobPortal.Domain.Entities;
using JobPortal.Domain.Enums;

namespace JobPortal.Application.Service;

public class JobApplicationService : IJobApplicationService
{
    private readonly IJobApplicationRepository _applications;
    private readonly IVacancyRepository _vacancies;
    private readonly ICompanyRepository _companies;
    private readonly IUnitOfWork _uow;
    private readonly IApplicationNotifier _notifier;

    public JobApplicationService(
        IJobApplicationRepository applications,
        IVacancyRepository vacancies,
        ICompanyRepository companies,
        IUnitOfWork uow,
        IApplicationNotifier notifier)
    {
        _applications = applications;
        _vacancies = vacancies;
        _companies = companies;
        _uow = uow;
        _notifier = notifier;
    }

    public async Task<ApplicationDto> ApplyAsync(
        Guid candidateId, CreateApplicationDto dto, CancellationToken ct)
    {
        var vacancy = await _vacancies.GetByIdAsync(dto.VacancyId, ct);

        // Відгукнутись можна тільки на опубліковану. Закрита або чернетка —
        // наче її нема, інакше по прямому посиланню відгукнуться на чернетку.
        if (vacancy is null || vacancy.Status != VacancyStatus.Published)
            throw new NotFoundException("Вакансію не знайдено");

        // Зручна перевірка: у нормальному випадку одразу 409 замість помилки бази.
        if (await _applications.ExistsAsync(dto.VacancyId, candidateId, ct))
            throw new ConflictException("Ви вже відгукнулись на цю вакансію");

        var application = new JobApplication
        {
            VacancyId = dto.VacancyId,
            CandidateId = candidateId,
            CoverLetter = string.IsNullOrWhiteSpace(dto.CoverLetter)
                ? null
                : dto.CoverLetter.Trim()
        };

        await _applications.AddAsync(application, ct);

        await _uow.SaveChangesAsync(ct);

        // Перечитуємо з Include, щоб DTO мав назву вакансії й компанії.
        var saved = await _applications.GetByIdAsync(application.Id, ct);
        var result = Map(saved!);

        await _notifier.NotifyNewApplicationAsync(vacancy.Company.OwnerId, result, ct);

        return result;
    }

    public async Task<PagedResponse<ApplicationDto>> GetMyAsync(
        Guid candidateId, ApplicationFilter filter, CancellationToken ct)
    {
        var (items, total) = await _applications.GetByCandidateAsync(candidateId, filter, ct);

        return new PagedResponse<ApplicationDto>
        {
            Items = items.Select(Map).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task<PagedResponse<ApplicationDto>> GetByVacancyAsync(
        Guid employerId, Guid vacancyId, ApplicationFilter filter, CancellationToken ct)
    {
        await EnsureVacancyIsMineAsync(employerId, vacancyId, ct);

        var (items, total) = await _applications.GetByVacancyAsync(vacancyId, filter, ct);

        return new PagedResponse<ApplicationDto>
        {
            Items = items.Select(Map).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task<ApplicationDto> UpdateStatusAsync(
        Guid employerId, Guid applicationId, UpdateApplicationStatusDto dto, CancellationToken ct)
    {
        var application = await _applications.GetByIdAsync(applicationId, ct);

        if (application is null)
            throw new NotFoundException("Відгук не знайдено");

        await EnsureVacancyIsMineAsync(employerId, application.VacancyId, ct);

        application.Status = dto.Status;

        _applications.Update(application);
        await _uow.SaveChangesAsync(ct);

        var result = Map(application);

        // Кандидату — «вас запросили» без оновлення сторінки.
        await _notifier.NotifyStatusChangedAsync(application.CandidateId, result, ct);

        return result;
    }

    private async Task EnsureVacancyIsMineAsync(
        Guid employerId, Guid vacancyId, CancellationToken ct)
    {
        var vacancy = await _vacancies.GetByIdAsync(vacancyId, ct);

        if (vacancy is null)
            throw new NotFoundException("Вакансію не знайдено");

        var company = await _companies.GetByOwnerAsync(employerId, ct);

        if (company is null || vacancy.CompanyId != company.Id)
            throw new ForbiddenException("Це не ваша вакансія");
    }

    private static ApplicationDto Map(JobApplication a) => new()
    {
        Id = a.Id,
        Status = a.Status,
        CoverLetter = a.CoverLetter,
        CreatedAt = a.CreatedAt,

        VacancyId = a.VacancyId,
        VacancyTitle = a.Vacancy.Title,
        CompanyName = a.Vacancy.Company.Name,

        CandidateId = a.CandidateId,
        CandidateFirstName = a.Candidate.FirstName,
        CandidateLastName = a.Candidate.LastName,
        CandidateEmail = a.Candidate.Email,
        CandidatePhone = a.Candidate.PhoneNumber
    };
}