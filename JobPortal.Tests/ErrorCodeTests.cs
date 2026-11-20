using System.Net.Http.Json;
using JobPortal.Tests.Setup;

namespace JobPortal.Tests;

public class ErrorCodeTests : ApiTestBase
{
    public ErrorCodeTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Applying_without_cv_returns_cv_required()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        var vacancyId = await CreateVacancyAsync(employer);

        var response = await ClientFor(candidate)
            .PostAsJsonAsync("/api/Application", new { vacancyId });

        Assert.Equal("cv_required", await ReadAsync(response, "code"));
    }

    [Fact]
    public async Task Second_application_returns_duplicate_application()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "CV");

        var vacancyId = await CreateVacancyAsync(employer);
        await ApplyAsync(candidate, vacancyId);

        var again = await ClientFor(candidate)
            .PostAsJsonAsync("/api/Application", new { vacancyId });

        Assert.Equal("duplicate_application", await ReadAsync(again, "code"));
    }

    [Fact]
    public async Task Second_favorite_returns_duplicate_favorite()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        var vacancyId = await CreateVacancyAsync(employer);

        await ClientFor(candidate).PostAsync($"/api/Favorite/{vacancyId}", null);

        var again = await ClientFor(candidate).PostAsync($"/api/Favorite/{vacancyId}", null);

        Assert.Equal("duplicate_favorite", await ReadAsync(again, "code"));
    }

    [Fact]
    public async Task Bad_file_returns_a_cv_specific_code()
    {
        var candidate = await RegisterCandidateAsync();

        var empty = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv", FileForm([], "e.pdf", "application/pdf"));

        var docx = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv", FileForm("x"u8.ToArray(), "cv.docx", "text/plain"));

        var fake = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv",
                FileForm([0x4D, 0x5A, 0x90, 0x00, 0x03], "virus.pdf", "application/pdf"));

        Assert.Equal("cv_empty", await ReadAsync(empty, "code"));
        Assert.Equal("cv_not_pdf", await ReadAsync(docx, "code"));
        Assert.Equal("cv_not_pdf", await ReadAsync(fake, "code"));
    }

    [Fact]
    public async Task Duplicate_email_returns_email_taken()
    {
        var email = Email("twice");

        var body = new { email, password = Password, firstName = "Twice", lastName = "Testov" };

        var first = await Anonymous().PostAsJsonAsync("/api/Auth/register/candidate", body);
        first.EnsureSuccessStatusCode();

        var second = await Anonymous().PostAsJsonAsync("/api/Auth/register/candidate", body);

        Assert.Equal("email_taken", await ReadAsync(second, "code"));
    }

    [Fact]
    public async Task Not_found_and_forbidden_have_generic_codes()
    {
        var owner = await RegisterEmployerAsync();
        var stranger = await RegisterEmployerAsync();

        var vacancyId = await CreateVacancyAsync(owner);

        var forbidden = await ClientFor(stranger).PostAsync($"/api/Vacancy/{vacancyId}/close", null);
        var notFound = await ClientFor(owner).PostAsync($"/api/Vacancy/{Guid.NewGuid()}/close", null);

        Assert.Equal("forbidden", await ReadAsync(forbidden, "code"));
        Assert.Equal("not_found", await ReadAsync(notFound, "code"));
    }
}