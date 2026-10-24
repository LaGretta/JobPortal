using JobPortal.Application.DTO.Companies;
using JobPortal.Application.Exceptions;
using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Service;
using JobPortal.Domain.Entities;

namespace JobPortal.Application.Service;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companies;
    private readonly IUnitOfWork _uow;

    public CompanyService(ICompanyRepository companies, IUnitOfWork uow)
    {
        _companies = companies;
        _uow = uow;
    }

    public async Task<CompanyDto> GetMyAsync(Guid ownerId, CancellationToken ct)
    {
        var company = await _companies.GetByOwnerAsync(ownerId, ct);

        if (company is null)
            throw new NotFoundException("Компанію не знайдено");

        return Map(company);
    }

    public async Task<CompanyDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var company = await _companies.GetByIdAsync(id, ct);

        if (company is null)
            throw new NotFoundException("Компанію не знайдено");

        return Map(company);
    }

    public async Task<CompanyDto> UpdateMyAsync(
        Guid ownerId, UpdateCompanyDto dto, CancellationToken ct)
    {
        // Шукаємо ПО ВЛАСНИКУ не по Id з запиту
        // Тому редагувати чужу компанію неможливо
        // не треба окремої перевірки - це моє
        var company = await _companies.GetByOwnerAsync(ownerId, ct);

        if (company is null)
            throw new NotFoundException("Компанію не знайдено");

        company.Name = dto.Name.Trim();
        company.Website = dto.Website?.Trim();
        company.Description = dto.Description?.Trim();
        company.LogoUrl = dto.LogoUrl?.Trim();

        _companies.Update(company);
        await _uow.SaveChangesAsync(ct);

        return Map(company);
    }

    private static CompanyDto Map(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Website = c.Website,
        Description = c.Description,
        LogoUrl = c.LogoUrl,
        CreatedAt = c.CreatedAt
    };
}