using JobPortal.Application.Interfaces.Infrastructure;
using JobPortal.Infrastructure.Data;
using JobPortal.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobPortal.Tests.Setup;


public class ApiFactory : WebApplicationFactory<Program>
{
    private const string LocalDbServer =
        @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";
    
    // Публічні константи, бо тести ними користуються: логін адміном
    // і складання протермінованого токена тим самим ключем.
    public const string JwtKey = "jobportal-tests-key-at-least-32-bytes-long";
    public const string JwtIssuer = "JobPortal.Tests";
    public const string JwtAudience = "JobPortal.Tests";

    public const string AdminEmail = "admin@tests.local";
    public const string AdminPassword = "AdminPass123!";

    private readonly string _dbName = "JobPortalTests_" + Guid.NewGuid().ToString("N");

    public FakeFileStorage Files { get; } = new();
    public FakeEmailSender Emails { get; } = new();

    public ApiFactory()
    {

        Set("ASPNETCORE_ENVIRONMENT", "Testing");

        Set("ConnectionStrings__Default", $"{LocalDbServer};Database={_dbName}");

        Set("Jwt__Key", "jobportal-tests-key-at-least-32-bytes-long");
        Set("Jwt__Issuer", "JobPortal.Tests");
        Set("Jwt__Audience", "JobPortal.Tests");
        Set("Jwt__AccessTokenMinutes", "15");
        Set("Jwt__RefreshTokenDays", "7");

        Set("BlobStorage__ConnectionString",
            "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=dGVzdA==;EndpointSuffix=core.windows.net");
        Set("BlobStorage__ContainerName", "tests");

        Set("Email__Host", "localhost");
        Set("Email__Port", "25");
        Set("Email__User", "tests");
        Set("Email__Password", "tests");
        Set("Email__FromName", "JobPortal Tests");
        Set("Email__FromAddress", "tests@example.com");
        Set("Email__ResetUrlBase", "http://localhost/reset");
        
        
        Set("Admin__Email", "admin@tests.local");
        Set("Admin__Password", "AdminPass123!");
        Set("Admin__FirstName", "Admin");
        Set("Admin__LastName", "Tests");
        
        
        Set("Jwt__Key", JwtKey);
        Set("Jwt__Issuer", JwtIssuer);
        Set("Jwt__Audience", JwtAudience);
        Set("Jwt__AccessTokenMinutes", "15");
        Set("Jwt__RefreshTokenDays", "7");

        Set("Admin__Email", AdminEmail);
        Set("Admin__Password", AdminPassword);
        Set("Admin__FirstName", "Admin");
        Set("Admin__LastName", "Tests");
    }

    private static void Set(string key, string value)
        => Environment.SetEnvironmentVariable(key, value);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IFileStorage>();
            services.AddSingleton<IFileStorage>(Files);

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
        });
    }

    // Міграції. MigrateAsync сам створює базу, якщо її нема.
    public async Task CreateDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
    
    // Сіємо після міграцій. При старті хоста бази ще не існує, тому
    // AdminSeeder там виходить ні з чим — досіваємо вручну.
    public async Task SeedAdminAsync()
    {
        using var scope = Services.CreateScope();
        await AdminSeeder.SeedAsync(scope.ServiceProvider, CancellationToken.None);
    }
    
    
    public async Task DropDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
    }
}