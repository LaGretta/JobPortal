using JobPortal.Application.DTO.Applications;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly AppDbContext _db;

    public JobApplicationRepository(AppDbContext db) => _db = db;
    
    private IQueryable<JobApplication> WithDetails()
        => _db.JobApplications
              .Include(x => x.Vacancy).ThenInclude(v => v.Company)
              .Include(x => x.Candidate);

    public Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken ct)
        => WithDetails().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExistsAsync(Guid vacancyId, Guid candidateId, CancellationToken ct)
        => _db.JobApplications.AnyAsync(
            x => x.VacancyId == vacancyId && x.CandidateId == candidateId, ct);

    public async Task<(List<JobApplication> Items, int Total)> GetByCandidateAsync(
        Guid candidateId, ApplicationFilter filter, CancellationToken ct)
    {
        var query = WithDetails().Where(x => x.CandidateId == candidateId);

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status);

        int total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(List<JobApplication> Items, int Total)> GetByVacancyAsync(
        Guid vacancyId, ApplicationFilter filter, CancellationToken ct)
    {
        var query = WithDetails().Where(x => x.VacancyId == vacancyId);

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status);

        int total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(JobApplication application, CancellationToken ct)
        => await _db.JobApplications.AddAsync(application, ct);

    public void Update(JobApplication application)
        => _db.JobApplications.Update(application);
}