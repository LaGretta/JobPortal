using System.Net;
using System.Net.Http.Json;
using JobPortal.Tests.Setup;

namespace JobPortal.Tests;

public class SecurityGapsTests : ApiTestBase
{
    public SecurityGapsTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Role_cannot_be_chosen_at_registration()
    {
        var email = Email("sneaky");

        var response = await Anonymous().PostAsJsonAsync("/api/Auth/register/candidate", new
        {
            email,
            password = Password,
            firstName = "Sneaky",
            lastName = "Testov",
            role = 2
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"role\":0", body);
        Assert.DoesNotContain("\"role\":2", body);

        var token = await ReadAsync(response, "accessToken");
        var admin = await ClientFor(token).GetAsync("/api/Admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, admin.StatusCode);
    }

    [Fact]
    public async Task Employer_changes_only_their_own_company()
    {
        var first = await RegisterEmployerAsync();
        var second = await RegisterEmployerAsync();

        var firstName = "Company-" + Guid.NewGuid().ToString("N")[..8];
        var secondName = "Company-" + Guid.NewGuid().ToString("N")[..8];

        var a = await ClientFor(first).PutAsJsonAsync("/api/Company/my", new { name = firstName });
        var b = await ClientFor(second).PutAsJsonAsync("/api/Company/my", new { name = secondName });

        a.EnsureSuccessStatusCode();
        b.EnsureSuccessStatusCode();

        Assert.Equal(firstName, await ReadAsync(
            await ClientFor(first).GetAsync("/api/Company/my"), "name"));

        Assert.Equal(secondName, await ReadAsync(
            await ClientFor(second).GetAsync("/api/Company/my"), "name"));
    }

    [Fact]
    public async Task Empty_file_is_rejected()
    {
        var candidate = await RegisterCandidateAsync();

        var response = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv", FileForm([], "empty.pdf", "application/pdf"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Expired_token_gives_unauthorized()
    {
        var response = await ClientFor(ExpiredToken()).GetAsync("/api/Account/me");
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tampered_token_gives_unauthorized()
    {
        var candidate = await RegisterCandidateAsync();

        var broken = candidate[..^1] + (candidate[^1] == 'a' ? 'b' : 'a');

        var response = await ClientFor(broken).GetAsync("/api/Account/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}