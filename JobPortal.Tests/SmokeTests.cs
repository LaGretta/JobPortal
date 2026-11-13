using System.Net;
using JobPortal.Tests.Setup;

namespace JobPortal.Tests;

public class SmokeTests : ApiTestBase
{
    public SmokeTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Candidate_can_register_and_read_own_empty_profile()
    {
        var token = await RegisterCandidateAsync();

        var response = await ClientFor(token).GetAsync("/api/Profile/my");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Profile_requires_authentication()
    {
        var response = await Anonymous().GetAsync("/api/Profile/my");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}