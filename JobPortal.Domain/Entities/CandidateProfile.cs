namespace JobPortal.Domain.Entities;

public class CandidateProfile
{
    public Guid Id { get; set; }                      

    public Guid UserId { get; set; }                  
    public User User { get; set; } = null!;           

    public string? Headline { get; set; }             // "Junior .NET Developer" рядок під іменем
    public string? About { get; set; }                // текст про себе

    public string? CvFileName { get; set; }           // оригінальна назва файлу, щоб віддати її при скачуванні
    public string? CvBlobPath { get; set; }           // шлях у Blob Storage, напр. "cv/{userId}/{guid}.pdf"
    public string? CvContentType { get; set; }        // "application/pdf" — віддаємо у відповіді
    public long? CvSizeBytes { get; set; }            // розмір показати на фронті й перевірити
    public DateTime? CvUploadedAt { get; set; }       // коли завантажив

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }         
}