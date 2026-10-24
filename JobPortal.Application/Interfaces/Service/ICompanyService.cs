using JobPortal.Application.DTO.Companies;

namespace JobPortal.Application.Interfaces.Service;

public interface ICompanyService
{
    Task<CompanyDto> GetMyAsync(Guid ownerId, CancellationToken ct);
    Task<CompanyDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<CompanyDto> UpdateMyAsync(Guid ownerId, UpdateCompanyDto dto, CancellationToken ct);
}