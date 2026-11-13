using System.Net;
using System.Net.Http.Json;
using JobPortal.Tests.Setup;

namespace JobPortal.Tests;

public class AccessControlTests : ApiTestBase
{
    public AccessControlTests(ApiFixture fixture) : base(fixture) { }

    // Готує повний сценарій: роботодавець, його вакансія,
    // кандидат з резюме і відгук на цю вакансію.
    private async Task<(string Employer, string Candidate, Guid VacancyId, Guid ApplicationId)>
        ArrangeApplicationAsync()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "CV");

        var vacancyId = await CreateVacancyAsync(employer);
        var applicationId = await ApplyAsync(candidate, vacancyId);

        return (employer, candidate, vacancyId, applicationId);
    }

    [Fact]
    public async Task Another_employer_cannot_download_the_cv()
    {
        var (_, _, _, applicationId) = await ArrangeApplicationAsync();

        var stranger = await RegisterEmployerAsync();

        var response = await ClientFor(stranger)
            .GetAsync($"/api/Application/{applicationId}/cv");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Candidate_cannot_use_the_employer_download_endpoint()
    {
        var (_, candidate, _, applicationId) = await ArrangeApplicationAsync();

        // Навіть власний відгук: цей ендпоінт тільно для роботодавця.
        var response = await ClientFor(candidate)
            .GetAsync($"/api/Application/{applicationId}/cv");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_cannot_download_the_cv()
    {
        var (_, _, _, applicationId) = await ArrangeApplicationAsync();

        var response = await Anonymous().GetAsync($"/api/Application/{applicationId}/cv");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_application_gives_not_found()
    {
        var employer = await RegisterEmployerAsync();

        var response = await ClientFor(employer)
            .GetAsync($"/api/Application/{Guid.NewGuid()}/cv");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Another_employer_cannot_see_applications_to_the_vacancy()
    {
        var (_, _, vacancyId, _) = await ArrangeApplicationAsync();

        var stranger = await RegisterEmployerAsync();

        var response = await ClientFor(stranger)
            .GetAsync($"/api/Application/vacancy/{vacancyId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Another_employer_cannot_change_application_status()
    {
        var (_, _, _, applicationId) = await ArrangeApplicationAsync();

        var stranger = await RegisterEmployerAsync();

        var response = await ClientFor(stranger)
            .PutAsJsonAsync($"/api/Application/{applicationId}/status", new { status = 2 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Another_employer_cannot_edit_the_vacancy()
    {
        var owner = await RegisterEmployerAsync();
        var vacancyId = await CreateVacancyAsync(owner);

        var stranger = await RegisterEmployerAsync();

        var response = await ClientFor(stranger).PutAsJsonAsync($"/api/Vacancy/{vacancyId}", new
        {
            title = "Перехоплено",
            description = "Опис вакансії, достатньо довгий для валідатора.",
            city = "Lublin",
            currency = 0,
            employmentType = 0,
            workMode = 1,
            category = 0,
            experienceLevel = 1,
            skills = new[] { "C#" }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Another_employer_cannot_close_the_vacancy()
    {
        var owner = await RegisterEmployerAsync();
        var vacancyId = await CreateVacancyAsync(owner);

        var stranger = await RegisterEmployerAsync();

        var response = await ClientFor(stranger)
            .PostAsync($"/api/Vacancy/{vacancyId}/close", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Candidate_cannot_create_a_vacancy()
    {
        var candidate = await RegisterCandidateAsync();

        var response = await ClientFor(candidate).PostAsJsonAsync("/api/Vacancy", new
        {
            title = "Я кандидат",
            description = "Опис вакансії, достатньо довгий для валідатора.",
            city = "Lublin",
            currency = 0,
            employmentType = 0,
            workMode = 1,
            category = 0,
            experienceLevel = 1,
            skills = new[] { "C#" }
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Employer_cannot_use_candidate_endpoints()
    {
        var employer = await RegisterEmployerAsync();

        var profile = await ClientFor(employer).GetAsync("/api/Profile/my");
        Assert.Equal(HttpStatusCode.Forbidden, profile.StatusCode);

        var favorites = await ClientFor(employer).GetAsync("/api/Favorite/my");
        Assert.Equal(HttpStatusCode.Forbidden, favorites.StatusCode);
    }

    [Fact]
    public async Task Each_candidate_gets_only_their_own_cv()
    {
        var first = await RegisterCandidateAsync();
        var second = await RegisterCandidateAsync();

        await UploadCvAsync(first, "FIRST-CANDIDATE");
        await UploadCvAsync(second, "SECOND-CANDIDATE");

        // Ендпоінт один і той самий, id ніде не передається —
        // користувач береться з токена. Саме це й перевіряємо.
        var firstCv = await ClientFor(first).GetAsync("/api/Profile/my/cv");
        var secondCv = await ClientFor(second).GetAsync("/api/Profile/my/cv");

        Assert.Contains("% FIRST-CANDIDATE", await firstCv.Content.ReadAsStringAsync());
        Assert.Contains("% SECOND-CANDIDATE", await secondCv.Content.ReadAsStringAsync());
    }
}