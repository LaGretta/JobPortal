namespace JobPortal.Domain.Entities;

public class VacancySkill
{
    public Guid Id { get; set; }

    public Guid VacancyId { get; set; }
    public Vacancy Vacancy { get; set; } = null!;

    public string Name { get; set; } = string.Empty;  
}