using JobPortal.Application.DTO.Admin;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _db;

    public AdminRepository(AppDbContext db) => _db = db;

    public async Task<(List<User> Items, int Total)> GetUsersAsync(
        AdminUserFilter filter, CancellationToken ct)
    {
        var query = _db.Users.Include(x => x.Company).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();

            query = query.Where(x =>
                x.Email.Contains(s) ||
                x.FirstName.Contains(s) ||
                x.LastName.Contains(s));
        }

        if (filter.Role.HasValue)
            query = query.Where(x => x.Role == filter.Role);

        int total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(List<Vacancy> Items, int Total)> GetVacanciesAsync(
        AdminVacancyFilter filter, CancellationToken ct)
    {
        var query = _db.Vacancies
            .Include(x => x.Company)
            .Include(x => x.Skills)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();


            query = query.Where(x => x.Title.Contains(s) || x.Company.Name.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(x => x.City == filter.City);

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
}