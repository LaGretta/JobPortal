using JobPortal.Application.Interfaces.Service;
using JobPortal.Application.Service;
using Microsoft.Extensions.DependencyInjection;

namespace JobPortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
        
        services.AddScoped<IVacancyService, VacancyService>();
        
        return services;
    }
}