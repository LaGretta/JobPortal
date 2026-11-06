using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Applications;
using JobPortal.Application.DTO.Notifications;
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
    private readonly INotificationQueue _notifications;
    private readonly ICandidateProfileRepository _profiles;
    private readonly IFileStorage _storage;

    public JobApplicationService(
        IJobApplicationRepository applications,
        IVacancyRepository vacancies,
        ICompanyRepository companies,
        IUnitOfWork uow,
        INotificationQueue notifications,
        ICandidateProfileRepository profiles,
        IFileStorage storage)
    {
        _applications = applications;
        _vacancies = vacancies;
        _companies = companies;
        _uow = uow;
        _notifications = notifications;
        _profiles = profiles;
        _storage = storage;
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

        var profile = await _profiles.GetByUserAsync(candidateId, ct);

        // Без резюме відгук не має сенсу: роботодавцю нічого буде качати.
        if (profile is null || profile.CvBlobPath is null)
            throw new ArgumentException("Спершу завантажте резюме у профіль");

        // Копія файлу на момент відгуку. Свій Guid, а не Id відгуку — Id EF
        // генерує сам, і покладатись на те, що він уже є до SaveChanges, не варто.
        var snapshotPath = $"applications/{candidateId}/{Guid.NewGuid()}.pdf";

        await _storage.CopyAsync(profile.CvBlobPath, snapshotPath, ct);

        var application = new JobApplication
        {
            VacancyId = dto.VacancyId,
            CandidateId = candidateId,
            CoverLetter = string.IsNullOrWhiteSpace(dto.CoverLetter)
                ? null
                : dto.CoverLetter.Trim(),

            CvSnapshotBlobPath = snapshotPath,
            CvSnapshotFileName = profile.CvFileName,
            CvSnapshotContentType = profile.CvContentType
        };

        try
        {
            await _applications.AddAsync(application, ct);

            await _uow.SaveChangesAsync(ct);
        }
        catch
        {
            await _storage.DeleteAsync(snapshotPath, ct);
            throw;
        }
        var saved = await _applications.GetByIdAsync(application.Id, ct);
        var result = Map(saved!);

        _notifications.Enqueue(new NotificationMessage(
            NotificationType.NewApplication, vacancy.Company.OwnerId, result));

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

        _notifications.Enqueue(new NotificationMessage(
            NotificationType.ApplicationStatusChanged, application.CandidateId, result));

        return result;
    }
    public async Task<FileDownloadDto> DownloadCvAsync(
        Guid employerId, Guid applicationId, CancellationToken ct)
    {
        var application = await _applications.GetByIdAsync(applicationId, ct);

        if (application is null)
            throw new NotFoundException("Відгук не знайдено");

        // Та сама перевірка, що в UpdateStatusAsync: вакансія мусить бути твоя.
        // Саме вона й робить резюме доступним тільно тому роботодавцю,
        // якому кандидат подався.
        await EnsureVacancyIsMineAsync(employerId, application.VacancyId, ct);

        // Беремо шлях З ВІДГУКУ, а не з профілю кандидата. Тому:
        // кандидат може перезавантажити або видалити своє резюме —
        // роботодавець і далі отримає те, що йому надіслали.
        if (application.CvSnapshotBlobPath is null)
            throw new NotFoundException("До цього відгуку резюме не додано");

        return new FileDownloadDto
        {
            Content = await _storage.DownloadAsync(application.CvSnapshotBlobPath, ct),
            FileName = application.CvSnapshotFileName ?? "cv.pdf",
            ContentType = application.CvSnapshotContentType ?? "application/pdf"
        };
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
        CandidatePhone = a.Candidate.PhoneNumber,
        
        HasCv = a.CvSnapshotBlobPath is not null,
        CvFileName = a.CvSnapshotFileName
    };
}