using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Infrastructure;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Security;
using JobPortal.Infrastructure.Data;
using JobPortal.Infrastructure.Email;
using JobPortal.Infrastructure.Repository;
using JobPortal.Infrastructure.Secutiry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JobPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);
        services.AddSingleton<ITokenLifetimeProvider>(sp => sp.GetRequiredService<JwtSettings>());
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value);
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        return services;
    }
}