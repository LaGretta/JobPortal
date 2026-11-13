using System.Net;
using System.Net.Http.Json;
using JobPortal.Tests.Setup;

namespace JobPortal.Tests;

public class CvStorageTests : ApiTestBase
{
    public CvStorageTests(ApiFixture fixture) : base(fixture) { }

    // Сховище одне на всі тести колекції, тому рахуємо різницю,
    // а не абсолютне число. Тести в одній колекції xUnit виконує
    // послідовно, тому різниця достовірна.

    [Fact]
    public async Task Replacing_cv_removes_the_old_file()
    {
        var candidate = await RegisterCandidateAsync();

        var before = Files.Count;

        await UploadCvAsync(candidate, "FIRST");
        Assert.Equal(before + 1, Files.Count);

        await UploadCvAsync(candidate, "SECOND");

        // Плюс один, а не плюс два: старий файл мусив зникнути.
        Assert.Equal(before + 1, Files.Count);
    }

    [Fact]
    public async Task Deleting_cv_removes_the_file()
    {
        var candidate = await RegisterCandidateAsync();

        var before = Files.Count;

        await UploadCvAsync(candidate, "X");
        Assert.Equal(before + 1, Files.Count);

        var deleted = await ClientFor(candidate).DeleteAsync("/api/Profile/my/cv");

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(before, Files.Count);
    }

    [Fact]
    public async Task Rejected_upload_leaves_nothing_in_storage()
    {
        var candidate = await RegisterCandidateAsync();

        var before = Files.Count;

        // .exe, перейменований у .pdf, з підробленим Content-Type.
        var fake = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03 };

        var response = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv", FileForm(fake, "virus.pdf", "application/pdf"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, Files.Count);
    }

    [Fact]
    public async Task Rejected_upload_does_not_touch_the_existing_cv()
    {
        var candidate = await RegisterCandidateAsync();

        var good = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv", PdfForm("GOOD", "good.pdf"));
        good.EnsureSuccessStatusCode();

        var bad = await ClientFor(candidate)
            .PostAsync("/api/Profile/my/cv", FileForm("not a pdf"u8.ToArray(), "bad.pdf", "application/pdf"));
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);

        // Профіль мусить лишитись на старому файлі.
        var profile = await ClientFor(candidate).GetAsync("/api/Profile/my");
        Assert.Equal("good.pdf", await ReadAsync(profile, "cvFileName"));

        var download = await ClientFor(candidate).GetAsync("/api/Profile/my/cv");
        Assert.Contains("% GOOD", await download.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Failed_duplicate_application_does_not_leave_a_copy()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "CV");

        var vacancyId = await CreateVacancyAsync(employer);

        var afterUpload = Files.Count;

        await ApplyAsync(candidate, vacancyId);

        // Резюме плюс його копія у відгуку.
        Assert.Equal(afterUpload + 1, Files.Count);

        var duplicate = await ClientFor(candidate)
            .PostAsJsonAsync("/api/Application", new { vacancyId, coverLetter = "ще раз" });

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        // Другий відгук не зберігся — значить і його копія резюме
        // не має лишитись у сховищі.
        Assert.Equal(afterUpload + 1, Files.Count);
    }

    [Fact]
    public async Task Parallel_applications_leave_exactly_one_copy()
    {
        var employer = await RegisterEmployerAsync();
        var candidate = await RegisterCandidateAsync();

        await UploadCvAsync(candidate, "CV");

        var vacancyId = await CreateVacancyAsync(employer);

        var afterUpload = Files.Count;

        var client = ClientFor(candidate);
        var responses = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            client.PostAsJsonAsync("/api/Application", new { vacancyId })));

        Assert.Single(responses.Where(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(4, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        
        Assert.Equal(afterUpload + 1, Files.Count);
    }
}