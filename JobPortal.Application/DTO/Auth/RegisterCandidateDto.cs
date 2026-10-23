namespace JobPortal.Application.DTO.Auth;

public class RegisterCandidateDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class RegisterEmployerDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyWebsite { get; set; }
    public string? CompanyDescription { get; set; }
}