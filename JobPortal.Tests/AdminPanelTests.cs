using System.Net;
using System.Net.Http.Json;
using JobPortal.Tests.Setup;

namespace JobPortal.Tests;

public class AdminPanelTests : ApiTestBase
{
    public AdminPanelTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Admin_is_seeded_and_can_log_in()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/Auth/login", new
        {
            email = ApiFactory.AdminEmail,
            password = ApiFactory.AdminPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"role\":2", body);
    }

    [Fact]
    public async Task Candidate_cannot_reach_the_admin_api()
    {
        var candidate = await RegisterCandidateAsync();

        var users = await ClientFor(candidate).GetAsync("/api/Admin/users");
        var vacancies = await ClientFor(candidate).GetAsync("/api/Admin/vacancies");

        Assert.Equal(HttpStatusCode.Forbidden, users.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, vacancies.StatusCode);
    }

    [Fact]
    public async Task Employer_cannot_reach_the_admin_api()
    {
        var employer = await RegisterEmployerAsync();

        var response = await ClientFor(employer).GetAsync("/api/Admin/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_cannot_reach_the_admin_api()
    {
        var response = await Anonymous().GetAsync("/api/Admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_sees_users_and_can_filter_by_role()
    {
        await RegisterCandidateAsync();
        await RegisterEmployerAsync();

        var admin = await LoginAdminAsync();

        var all = await ClientFor(admin).GetAsync("/api/Admin/users?pageSize=50");
        Assert.Equal(HttpStatusCode.OK, all.StatusCode);

        var employers = await ClientFor(admin).GetAsync("/api/Admin/users?role=1&pageSize=50");
        var body = await employers.Content.ReadAsStringAsync();

        Assert.DoesNotContain("\"role\":0", body);
        Assert.Contains("\"role\":1", body);
    }

    [Fact]
    public async Task Admin_can_search_users_by_email()
    {
        var email = Email("needle");

        var registered = await Anonymous().PostAsJsonAsync("/api/Auth/register/candidate", new
        {
            email,
            password = Password,
            firstName = "Needle",
            lastName = "Testov"
        });
        registered.EnsureSuccessStatusCode();

        var admin = await LoginAdminAsync();

        var found = await ClientFor(admin).GetAsync($"/api/Admin/users?search={email}");

        Assert.Equal("1", await ReadAsync(found, "totalCount"));
    }

    [Fact]
    public async Task Admin_user_list_does_not_leak_password_hashes()
    {
        await RegisterCandidateAsync();

        var admin = await LoginAdminAsync();

        var response = await ClientFor(admin).GetAsync("/api/Admin/users?pageSize=50");
        var body = await response.Content.ReadAsStringAsync();


        Assert.DoesNotContain("assword", body);
        Assert.DoesNotContain("$2a$", body);
    }

    [Fact]
    public async Task Admin_sees_drafts_and_closed_vacancies()
    {
        var employer = await RegisterEmployerAsync();

        var city = "AdmCity" + Guid.NewGuid().ToString("N")[..8];

        var published = await CreateVacancyAsync(employer, city: city);
        await CreateVacancyAsync(employer, publish: false, city: city);

        await ClientFor(employer).PostAsync($"/api/Vacancy/{published}/close", null);

        var publicSearch = await Anonymous().GetAsync($"/api/Vacancy/search?city={city}");
        Assert.Equal("0", await ReadAsync(publicSearch, "totalCount"));

        var admin = await LoginAdminAsync();

        var adminList = await ClientFor(admin).GetAsync($"/api/Admin/vacancies?city={city}&pageSize=50");
        Assert.Equal("2", await ReadAsync(adminList, "totalCount"));
    }

    [Fact]
    public async Task Admin_can_filter_vacancies_by_status()
    {
        var employer = await RegisterEmployerAsync();

        var city = "AdmCity" + Guid.NewGuid().ToString("N")[..8];

        await CreateVacancyAsync(employer, city: city);
        await CreateVacancyAsync(employer, publish: false, city: city);

        var admin = await LoginAdminAsync();

        var drafts = await ClientFor(admin).GetAsync($"/api/Admin/vacancies?city={city}&status=0");
        var live = await ClientFor(admin).GetAsync($"/api/Admin/vacancies?city={city}&status=1");

        Assert.Equal("1", await ReadAsync(drafts, "totalCount"));
        Assert.Equal("1", await ReadAsync(live, "totalCount"));
    }

    [Fact]
    public async Task Admin_can_close_any_vacancy()
    {
        var employer = await RegisterEmployerAsync();

        var city = "AdmCity" + Guid.NewGuid().ToString("N")[..8];
        var vacancyId = await CreateVacancyAsync(employer, city: city);

        var admin = await LoginAdminAsync();

        var closed = await ClientFor(admin).PostAsync($"/api/Admin/vacancies/{vacancyId}/close", null);

        Assert.Equal(HttpStatusCode.OK, closed.StatusCode);
        Assert.Equal("2", await ReadAsync(closed, "status"));

        var search = await Anonymous().GetAsync($"/api/Vacancy/search?city={city}");
        Assert.Equal("0", await ReadAsync(search, "totalCount"));

        var mine = await ClientFor(employer).GetAsync("/api/Vacancy/my?pageSize=50");
        Assert.Equal(HttpStatusCode.OK, mine.StatusCode);
    }

    [Fact]
    public async Task Closing_an_already_closed_vacancy_is_idempotent()
    {
        var employer = await RegisterEmployerAsync();
        var vacancyId = await CreateVacancyAsync(employer);

        var admin = await LoginAdminAsync();

        var first = await ClientFor(admin).PostAsync($"/api/Admin/vacancies/{vacancyId}/close", null);
        var second = await ClientFor(admin).PostAsync($"/api/Admin/vacancies/{vacancyId}/close", null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task Closing_an_unknown_vacancy_gives_not_found()
    {
        var admin = await LoginAdminAsync();

        var response = await ClientFor(admin).PostAsync($"/api/Admin/vacancies/{Guid.NewGuid()}/close", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}