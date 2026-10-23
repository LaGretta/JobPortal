using JobPortal.Domain.Enums;

namespace JobPortal.Application.Common;

public static class Roles
{
    public const string JobSeeker = nameof(UserRole.JobSeeker);
    public const string Employer = nameof(UserRole.Employer);
    public const string Admin = nameof(UserRole.Admin);
}