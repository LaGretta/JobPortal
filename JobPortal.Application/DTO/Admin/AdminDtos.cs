using JobPortal.Domain.Enums;

namespace JobPortal.Application.DTO.Admin;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? CompanyName { get; set; }


    public bool IsLockedOut { get; set; }
}

public class AdminUserFilter : PageRequest
{
    public string? Search { get; set; }
    public UserRole? Role { get; set; }
}

public class AdminVacancyFilter : PageRequest
{
    public string? Search { get; set; }
    public string? City { get; set; }

    public VacancyStatus? Status { get; set; }
}