using JobPortal.Application.DTO;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class FavoriteVacancyRepository : IFavoriteVacancyRepository
{
    private readonly AppDbContext _db;

    public FavoriteVacancyRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid userId, Guid vacancyId, CancellationToken ct)
        => _db.FavoriteVacancies.AnyAsync(
            x => x.UserId == userId && x.VacancyId == vacancyId, ct);

    public async Task<(List<FavoriteVacancy> Items, int Total)> GetByUserAsync(
        Guid userId, PageRequest page, CancellationToken ct)
    {
        var query = _db.FavoriteVacancies
            .Where(x => x.UserId == userId)
            .Include(x => x.Vacancy).ThenInclude(v => v.Company)
            .Include(x => x.Vacancy).ThenInclude(v => v.Skills);

        int total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(FavoriteVacancy favorite, CancellationToken ct)
        => await _db.FavoriteVacancies.AddAsync(favorite, ct);

    public Task<int> RemoveAsync(Guid userId, Guid vacancyId, CancellationToken ct)
        => _db.FavoriteVacancies
            .Where(x => x.UserId == userId && x.VacancyId == vacancyId)
            .ExecuteDeleteAsync(ct);
}