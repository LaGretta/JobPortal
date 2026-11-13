using System.Net;
using System.Net.Http.Json;
using JobPortal.Tests.Setup;

namespace JobPortal.Tests;

public class ApplicationRulesTests : ApiTestBase
{
    public ApplicationRulesTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Second_application_to_the_same_vacancy_is_rejected()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "CV");

        var vacancyId = await CreateVacancyAsync(employer);

        await ApplyAsync(candidate, vacancyId);

        var again = await ClientFor(candidate)
            .PostAsJsonAsync("/api/Application", new { vacancyId });

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);

        // І в списку рівно один відгук, а не два.
        var mine = await ClientFor(candidate).GetAsync("/api/Application/my");
        Assert.Equal("1", await ReadAsync(mine, "totalCount"));
    }

    [Fact]
    public async Task Cannot_apply_to_a_draft_vacancy()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "CV");

        // publish: false — вакансія лишається чернеткою.
        var vacancyId = await CreateVacancyAsync(employer, publish: false);

        var response = await ClientFor(candidate)
            .PostAsJsonAsync("/api/Application", new { vacancyId });

        // Саме 404, а не 403: чернетки для стороннього не існує.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_apply_to_a_closed_vacancy()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "CV");

        var vacancyId = await CreateVacancyAsync(employer);

        var closed = await ClientFor(employer).PostAsync($"/api/Vacancy/{vacancyId}/close", null);
        closed.EnsureSuccessStatusCode();

        var response = await ClientFor(candidate)
            .PostAsJsonAsync("/api/Application", new { vacancyId });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cannot_apply_without_a_cv()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        var vacancyId = await CreateVacancyAsync(employer);

        var response = await ClientFor(candidate)
            .PostAsJsonAsync("/api/Application", new { vacancyId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Closed_vacancy_disappears_from_search()
    {
        var employer = await RegisterEmployerAsync();

        var city = "City" + Guid.NewGuid().ToString("N")[..8];
        var vacancyId = await CreateVacancyAsync(employer, city: city);

        var before = await Anonymous().GetAsync($"/api/Vacancy/search?city={city}");
        Assert.Equal("1", await ReadAsync(before, "totalCount"));

        await ClientFor(employer).PostAsync($"/api/Vacancy/{vacancyId}/close", null);

        var after = await Anonymous().GetAsync($"/api/Vacancy/search?city={city}");
        Assert.Equal("0", await ReadAsync(after, "totalCount"));
    }

    [Fact]
    public async Task Closed_vacancy_is_not_available_by_direct_link()
    {
        var employer = await RegisterEmployerAsync();
        var vacancyId = await CreateVacancyAsync(employer);

        var before = await Anonymous().GetAsync($"/api/Vacancy/{vacancyId}");
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);

        await ClientFor(employer).PostAsync($"/api/Vacancy/{vacancyId}/close", null);

        // Закрита вакансія не просто зникає з пошуку — вона недоступна
        // і за прямим посиланням, яке хтось міг зберегти.
        var after = await Anonymous().GetAsync($"/api/Vacancy/{vacancyId}");
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
    }

    [Fact]
    public async Task Draft_vacancy_is_not_in_search()
    {
        var employer = await RegisterEmployerAsync();

        var city = "City" + Guid.NewGuid().ToString("N")[..8];
        await CreateVacancyAsync(employer, publish: false, city: city);

        var response = await Anonymous().GetAsync($"/api/Vacancy/search?city={city}");

        Assert.Equal("0", await ReadAsync(response, "totalCount"));
    }

    [Fact]
    public async Task Employer_sees_the_application_to_their_own_vacancy()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "CV");

        var vacancyId = await CreateVacancyAsync(employer);
        await ApplyAsync(candidate, vacancyId);

        var response = await ClientFor(employer)
            .GetAsync($"/api/Application/vacancy/{vacancyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("1", await ReadAsync(response, "totalCount"));
    }
}