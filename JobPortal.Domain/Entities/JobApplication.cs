using JobPortal.Domain.Enums;

namespace JobPortal.Domain.Entities;

public class JobApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VacancyId { get; set; }
    public Vacancy Vacancy { get; set; } = null!;

    public Guid CandidateId { get; set; }
    public User Candidate { get; set; } = null!;

    public string? CoverLetter { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    
    
    // Копія CV на момент відгуку кандидат потім міняє резюм роботодавець бачить те що йому надіслали а не нове
    public string? CvSnapshotBlobPath { get; set; }
    public string? CvSnapshotFileName { get; set; }
    public string? CvSnapshotContentType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}