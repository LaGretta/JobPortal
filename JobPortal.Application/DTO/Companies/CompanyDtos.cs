namespace JobPortal.Application.DTO.Companies;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
}