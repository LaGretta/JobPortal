using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using JobPortal.Tests.Fakes;

namespace JobPortal.Tests.Setup;

[Collection("api")]
public abstract class ApiTestBase
{
    protected readonly ApiFactory Factory;
    protected readonly FakeFileStorage Files;
    protected readonly FakeEmailSender Emails;

    protected ApiTestBase(ApiFixture fixture)
    {
        Factory = fixture.Factory;
        Files = fixture.Factory.Files;
        Emails = fixture.Factory.Emails;
    }

    protected const string Password = "TestPass123!";


    protected static string Email(string prefix)
        => $"{prefix}.{Guid.NewGuid():N}@example.com";

    protected HttpClient Anonymous() => Factory.CreateClient();

    protected HttpClient ClientFor(string accessToken)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    protected async Task<string> RegisterCandidateAsync()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/Auth/register/candidate", new
        {
            email = Email("cand"),
            password = Password,
            firstName = "Cand",
            lastName = "Testov"
        });

        response.EnsureSuccessStatusCode();

        return await ReadAsync(response, "accessToken");
    }

    protected async Task<string> RegisterEmployerAsync()
    {
        var response = await Anonymous().PostAsJsonAsync("/api/Auth/register/employer", new
        {
            email = Email("emp"),
            password = Password,
            firstName = "Emil",
            lastName = "Testov",
            companyName = "Co " + Guid.NewGuid().ToString("N")[..8]
        });

        response.EnsureSuccessStatusCode();

        return await ReadAsync(response, "accessToken");
    }

    // city: тестам на пошук треба своє місто, інакше вони побачать
    // вакансії, створені сусідніми тестами в тій самій базі.
    protected async Task<Guid> CreateVacancyAsync(
        string employerToken, bool publish = true, string? city = null)
    {
        var client = ClientFor(employerToken);

        var created = await client.PostAsJsonAsync("/api/Vacancy", new
        {
            title = "Test vacancy",
            description = "Опис вакансії, достатньо довгий для валідатора.",
            city = city ?? "Lublin",
            salaryMin = 5000,
            salaryMax = 9000,
            currency = 0,
            employmentType = 0,
            workMode = 1,
            category = 0,
            experienceLevel = 1,
            skills = new[] { "C#" }
        });

        created.EnsureSuccessStatusCode();

        var vacancyId = Guid.Parse(await ReadAsync(created, "id"));

        if (publish)
        {
            var published = await client.PostAsync($"/api/Vacancy/{vacancyId}/publish", null);
            published.EnsureSuccessStatusCode();
        }

        return vacancyId;
    }

    protected static MultipartFormDataContent PdfForm(
        string marker, string fileName = "cv.pdf", string contentType = "application/pdf")
    {
        var bytes = Encoding.UTF8.GetBytes($"%PDF-1.4\n% {marker}\n%%EOF\n");
        return FileForm(bytes, fileName, contentType);
    }

    protected static MultipartFormDataContent FileForm(
        byte[] bytes, string fileName, string contentType)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var form = new MultipartFormDataContent();

        form.Add(file, "file", fileName);

        return form;
    }

    protected async Task UploadCvAsync(string candidateToken, string marker)
    {
        var response = await ClientFor(candidateToken)
            .PostAsync("/api/Profile/my/cv", PdfForm(marker));

        response.EnsureSuccessStatusCode();
    }

    protected async Task<Guid> ApplyAsync(string candidateToken, Guid vacancyId)
    {
        var response = await ClientFor(candidateToken)
            .PostAsJsonAsync("/api/Application", new { vacancyId, coverLetter = "test" });

        response.EnsureSuccessStatusCode();

        return Guid.Parse(await ReadAsync(response, "id"));
    }
    
    protected static async Task<string> ReadAsync(HttpResponseMessage response, string property)
    {
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

        return body![property].ToString();
    }
}