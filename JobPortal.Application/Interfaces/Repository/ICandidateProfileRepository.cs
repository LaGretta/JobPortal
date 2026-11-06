using JobPortal.Domain.Entities;

namespace JobPortal.Application.Interfaces.Repository;

public interface ICandidateProfileRepository
{
    Task<CandidateProfile?> GetByUserAsync(Guid userId, CancellationToken ct);
    Task AddAsync(CandidateProfile profile, CancellationToken ct);
}