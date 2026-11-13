using System.Net;
using JobPortal.Tests.Setup;

namespace JobPortal.Tests;

public class CvSnapshotTests : ApiTestBase
{
    public CvSnapshotTests(ApiFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Employer_gets_the_cv_that_was_sent_not_the_current_one()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "FIRST");

        var vacancyId = await CreateVacancyAsync(employer);
        var applicationId = await ApplyAsync(candidate, vacancyId);

        await UploadCvAsync(candidate, "SECOND");

        var response = await ClientFor(employer)
            .GetAsync($"/api/Application/{applicationId}/cv");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("% FIRST", content);
        Assert.DoesNotContain("% SECOND", content);
    }

    [Fact]
    public async Task Employer_still_gets_the_cv_after_candidate_deletes_it()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "SENT");

        var vacancyId = await CreateVacancyAsync(employer);
        var applicationId = await ApplyAsync(candidate, vacancyId);

        var deleted = await ClientFor(candidate).DeleteAsync("/api/Profile/my/cv");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var own = await ClientFor(candidate).GetAsync("/api/Profile/my/cv");
        Assert.Equal(HttpStatusCode.NotFound, own.StatusCode);

        var response = await ClientFor(employer)
            .GetAsync($"/api/Application/{applicationId}/cv");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("% SENT", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Snapshot_keeps_the_file_name_from_the_moment_of_applying()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        var first = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv", PdfForm("FIRST", "staryi.pdf"));
        first.EnsureSuccessStatusCode();

        var vacancyId = await CreateVacancyAsync(employer);
        var applicationId = await ApplyAsync(candidate, vacancyId);

        var second = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv", PdfForm("SECOND", "novyi.pdf"));
        second.EnsureSuccessStatusCode();

        var response = await ClientFor(employer)
            .GetAsync($"/api/Application/{applicationId}/cv");

        Assert.Contains("staryi.pdf", response.Content.Headers.ContentDisposition!.ToString());
    }
}