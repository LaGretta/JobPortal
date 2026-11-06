using JobPortal.Application.DTO;
using JobPortal.Application.DTO.Profiles;
using JobPortal.Application.Exceptions;
using JobPortal.Application.Interfaces;
using JobPortal.Application.Interfaces.Infrastructure;
using JobPortal.Application.Interfaces.Repository;
using JobPortal.Application.Interfaces.Service;
using JobPortal.Domain.Entities;

namespace JobPortal.Application.Service;

public class CandidateProfileService : ICandidateProfileService
{
    // 5 МБ. Резюме на дві сторінки — це 200–500 КБ; 5 МБ з великим запасом.
    private const long MaxCvBytes = 5 * 1024 * 1024;
    private const string PdfContentType = "application/pdf";

    private readonly ICandidateProfileRepository _profiles;
    private readonly IFileStorage _storage;
    private readonly IUnitOfWork _uow;

    public CandidateProfileService(
        ICandidateProfileRepository profiles,
        IFileStorage storage,
        IUnitOfWork uow)
    {
        _profiles = profiles;
        _storage = storage;
        _uow = uow;
    }

    public async Task<CandidateProfileDto> GetMyAsync(Guid userId, CancellationToken ct)
    {
        var profile = await _profiles.GetByUserAsync(userId, ct);

        // Профіля ще нема — це не помилка, а порожній кабінет.
        // 404 тут змусив би фронт обробляти помилку замість показу пустої форми.
        return profile is null ? new CandidateProfileDto() : Map(profile);
    }

    public async Task<CandidateProfileDto> UpdateAsync(
        Guid userId, UpdateCandidateProfileDto dto, CancellationToken ct)
    {
        var profile = await GetOrCreateAsync(userId, ct);

        profile.Headline = Clean(dto.Headline);
        profile.About = Clean(dto.About);
        profile.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return Map(profile);
    }

    public async Task<CandidateProfileDto> UploadCvAsync(
        Guid userId, FileUploadDto file, CancellationToken ct)
    {
        Validate(file);

        var profile = await GetOrCreateAsync(userId, ct);

        // Запамʼятовуємо старий шлях ДО перезапису полів.
        var oldPath = profile.CvBlobPath;

        // Новий Guid у назві, а не ім'я файлу користувача: інакше
        // "резюме.pdf" від двох людей могли б зіткнутись, а ще й
        // ім'я файлу з кирилицею й пробілами в шляху — зайвий клопіт.
        var newPath = $"{userId}/{Guid.NewGuid()}.pdf";

        await _storage.UploadAsync(file.Content, newPath, PdfContentType, ct);

        profile.CvFileName = SafeFileName(file.FileName);
        profile.CvBlobPath = newPath;
        profile.CvContentType = PdfContentType;
        profile.CvSizeBytes = file.SizeBytes;
        profile.CvUploadedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        // Старий файл видаляємо ПІСЛЯ збереження бази. Якщо видалення впаде —
        // у сховищі лишиться сирота, але профіль буде правильний. Якби видаляли
        // спочатку, а база впала — профіль вказував би на файл, якого вже нема.
        // Знімки резюме в відгуках це не зачепить: вони лежать окремими копіями.
        if (!string.IsNullOrEmpty(oldPath))
            await _storage.DeleteAsync(oldPath, ct);

        return Map(profile);
    }

    public async Task DeleteCvAsync(Guid userId, CancellationToken ct)
    {
        var profile = await _profiles.GetByUserAsync(userId, ct);

        if (profile is null || profile.CvBlobPath is null)
            throw new NotFoundException("Резюме не завантажено");

        var path = profile.CvBlobPath;

        profile.CvFileName = null;
        profile.CvBlobPath = null;
        profile.CvContentType = null;
        profile.CvSizeBytes = null;
        profile.CvUploadedAt = null;
        profile.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        await _storage.DeleteAsync(path, ct);
    }

    public async Task<FileDownloadDto> DownloadMyCvAsync(Guid userId, CancellationToken ct)
    {
        var profile = await _profiles.GetByUserAsync(userId, ct);

        if (profile is null || profile.CvBlobPath is null)
            throw new NotFoundException("Резюме не завантажено");

        return new FileDownloadDto
        {
            Content = await _storage.DownloadAsync(profile.CvBlobPath, ct),
            FileName = profile.CvFileName ?? "cv.pdf",
            ContentType = profile.CvContentType ?? PdfContentType
        };
    }

    private async Task<CandidateProfile> GetOrCreateAsync(Guid userId, CancellationToken ct)
    {
        var profile = await _profiles.GetByUserAsync(userId, ct);

        if (profile is not null)
            return profile;

        // Створюємо при першому зверненні. Окремого "створити профіль"
        // на фронті не буде — користувач просто заповнює й зберігає.
        profile = new CandidateProfile { UserId = userId };
        await _profiles.AddAsync(profile, ct);

        return profile;
    }

    private static void Validate(FileUploadDto file)
    {
        if (file.SizeBytes == 0)
            throw new ArgumentException("Файл порожній");

        if (file.SizeBytes > MaxCvBytes)
            throw new ArgumentException($"Файл більший за {MaxCvBytes / 1024 / 1024} МБ");

        // Розширення й Content-Type прислав клієнт. Перевіряємо їх, щоб дати
        // зрозумілу помилку на звичайну помилку користувача (кинув .docx).
        if (!string.Equals(Path.GetExtension(file.FileName), ".pdf",
                StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Дозволено тільки PDF");

        if (!string.Equals(file.ContentType, PdfContentType,
                StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Дозволено тільки PDF");
        
        Span<byte> header = stackalloc byte[5];
        int read = file.Content.ReadAtLeast(header, 5, throwOnEndOfStream: false);

        // Відмотуємо назад — далі цей самий потік піде в сховище.
        file.Content.Seek(0, SeekOrigin.Begin);

        if (read < 5 || !header.SequenceEqual("%PDF-"u8))
            throw new ArgumentException("Файл не схожий на PDF");
    }

    private static string SafeFileName(string name)
    {
        var clean = Path.GetFileName(name);

        // 260 — це HasMaxLength(260) з конфігурації CandidateProfile.
        return clean.Length > 260 ? clean[..260] : clean;
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static CandidateProfileDto Map(CandidateProfile p) => new()
    {
        Headline = p.Headline,
        About = p.About,
        HasCv = p.CvBlobPath is not null,
        CvFileName = p.CvFileName,
        CvSizeBytes = p.CvSizeBytes,
        CvUploadedAt = p.CvUploadedAt
    };
}