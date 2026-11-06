using JobPortal.Application.DTO.Vacancies;
using JobPortal.Domain.Entities;

namespace JobPortal.Application.Mapping;

public static class VacancyMapper
{
    public static VacancyDto ToDto(Vacancy v) => new()
    {
        Id = v.Id,
        Title = v.Title,
        Description = v.Description,
        City = v.City,
        SalaryMin = v.SalaryMin,
        SalaryMax = v.SalaryMax,
        Currency = v.Currency,
        EmploymentType = v.EmploymentType,
        WorkMode = v.WorkMode,
        Category = v.Category,
        ExperienceLevel = v.ExperienceLevel,
        Skills = v.Skills.Select(s => s.Name).ToList(),
        Status = v.Status,
        CompanyId = v.CompanyId,
        CompanyName = v.Company.Name,
        CompanyLogoUrl = v.Company.LogoUrl,
        CreatedAt = v.CreatedAt,
        PublishedAt = v.PublishedAt
    };
}