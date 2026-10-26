using JobPortal.Application.DTO.Applications;
using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid vacancyId, Guid candidateId, CancellationToken ct);

    // Мої відгуки (здобувач)
    Task<(List<JobApplication> Items, int Total)> GetByCandidateAsync(
        Guid candidateId, ApplicationFilter filter, CancellationToken ct);
    // Відгуки на конкретну вакансію (роботодавець)
    Task<(List<JobApplication> Items, int Total)> GetByVacancyAsync(
        Guid vacancyId, ApplicationFilter filter, CancellationToken ct);

    Task AddAsync(JobApplication application, CancellationToken ct);
    void Update(JobApplication application);
}