namespace JobPortal.Domain.Entities;

public class FavoriteVacancy
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }                  
    public User User { get; set; } = null!;

    public Guid VacancyId { get; set; }              
    public Vacancy Vacancy { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}