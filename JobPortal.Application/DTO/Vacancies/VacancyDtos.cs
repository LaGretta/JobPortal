using JobPortal.Domain.Enums;

namespace JobPortal.Application.DTO.Vacancies;

public class VacancyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public Currency Currency { get; set; }

    public EmploymentType EmploymentType { get; set; }
    public WorkMode WorkMode { get; set; }
    public VacancyStatus Status { get; set; }

    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyLogoUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class CreateVacancyDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public Currency Currency { get; set; } = Currency.PLN;

    public EmploymentType EmploymentType { get; set; }
    public WorkMode WorkMode { get; set; }
}
public class UpdateVacancyDto : CreateVacancyDto { }


// Фільтри пошуку. Успадковує Page і PageSize з PageRequest,
// тому обмеження на 50 записів діє автоматично.
public class VacancyFilter : PageRequest
{
    public string? Search { get; set; }
    public string? City { get; set; }
    public EmploymentType? EmploymentType { get; set; }
    public WorkMode? WorkMode { get; set; }
    public decimal? SalaryFrom { get; set; }
    public Currency? Currency { get; set; }
}