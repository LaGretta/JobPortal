using JobPortal.Domain.Enums;

namespace JobPortal.Domain.Entities;

public class Vacancy
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    //Обидва nullable — бувають вакансії без вказаної зарплати.
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    
    public Currency Currency { get; set; } = Currency.PLN;

    public EmploymentType EmploymentType { get; set; }
    public WorkMode WorkMode { get; set; }
    public VacancyStatus Status { get; set; } = VacancyStatus.Draft;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    
    public List<JobApplication> Applications { get; set; } = new();
}