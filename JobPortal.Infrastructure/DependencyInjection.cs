using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Infrastructure;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Security;
using JobPortal.Infrastructure.Data;
using JobPortal.Infrastructure.Email;
using JobPortal.Infrastructure.Repository;
using JobPortal.Infrastructure.Secutiry;
using JobPortal.Infrastructure.Storage;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));
        services.AddSingleton<IFileStorage, BlobFileStorage>();


        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IVacancyRepository, VacancyRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IFavoriteVacancyRepository, FavoriteVacancyRepository>();
        services.AddScoped<ICandidateProfileRepository, CandidateProfileRepository>();
        
        
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<EmailSettings>>().Value);
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IFileStorage, BlobFileStorage>();
        var blobSection = configuration.GetSection("BlobStorage");
        var blobConnection = blobSection["ConnectionString"];

        
        // Падаємо на старті зі зрозумілим текстом. Інакше кожен запит до
        // профілю повертає 500 "An unexpected error occurred", а справжня
        // причина лежить у логах за сотнею рядків стеку.
        if (string.IsNullOrWhiteSpace(blobConnection) ||
            !blobConnection.StartsWith("DefaultEndpointsProtocol", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "BlobStorage:ConnectionString відсутній або це не повний рядок підключення " +
                "(має починатись на DefaultEndpointsProtocol=).");
        services.Configure<BlobStorageSettings>(blobSection);

        return services;
    }
}