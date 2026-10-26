using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Applications;

namespace JobPortal.Application.Interfaces.Service;

public interface IJobApplicationService
{
    Task<ApplicationDto> ApplyAsync(Guid candidateId, CreateApplicationDto dto, CancellationToken ct);

    Task<PagedResponse<ApplicationDto>> GetMyAsync(
        Guid candidateId, ApplicationFilter filter, CancellationToken ct);

    Task<PagedResponse<ApplicationDto>> GetByVacancyAsync(
        Guid employerId, Guid vacancyId, ApplicationFilter filter, CancellationToken ct);

    Task<ApplicationDto> UpdateStatusAsync(
        Guid employerId, Guid applicationId, UpdateApplicationStatusDto dto, CancellationToken ct);
}