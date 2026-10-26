using JobPortal.Domain.Enums;

namespace JobPortal.Application.DTO.Applications;

public class ApplicationDto
{
    public Guid Id { get; set; }
    public ApplicationStatus Status { get; set; }
    public string? CoverLetter { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid VacancyId { get; set; }
    public string VacancyTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;

    // Кандидат — роботодавцю видно, хто відгукнувся.
    public Guid CandidateId { get; set; }
    public string CandidateFirstName { get; set; } = string.Empty;
    public string CandidateLastName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string? CandidatePhone { get; set; }
}

public class CreateApplicationDto
{
    public Guid VacancyId { get; set; }
    public string? CoverLetter { get; set; }
}

public class UpdateApplicationStatusDto
{
    public ApplicationStatus Status { get; set; }
}

public class ApplicationFilter : PageRequest
{
    public ApplicationStatus? Status { get; set; }
}