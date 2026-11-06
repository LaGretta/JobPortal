using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class CandidateProfileRepository : ICandidateProfileRepository
{
    private readonly AppDbContext _db;

    public CandidateProfileRepository(AppDbContext db) => _db = db;

    public Task<CandidateProfile?> GetByUserAsync(Guid userId, CancellationToken ct)
        => _db.CandidateProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task AddAsync(CandidateProfile profile, CancellationToken ct)
        => await _db.CandidateProfiles.AddAsync(profile, ct);
}