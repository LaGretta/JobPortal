using JobPortal.Application.Interfaces.Repository;
using JobPortal.Domain.Entities;
using JobPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Repository;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _db;

    public CompanyRepository(AppDbContext db) => _db = db;

    public Task<Company?> GetByOwnerAsync(Guid ownerId, CancellationToken ct)
        => _db.Companies.FirstOrDefaultAsync(x => x.OwnerId == ownerId, ct);

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Companies.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Company company, CancellationToken ct)
        => await _db.Companies.AddAsync(company, ct);

    public void Update(Company company) => _db.Companies.Update(company);
}