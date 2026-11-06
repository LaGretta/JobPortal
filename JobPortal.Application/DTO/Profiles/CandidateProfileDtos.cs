namespace JobPortal.Application.DTO.Profiles;

public class CandidateProfileDto
{
    public string? Headline { get; set; }
    public string? About { get; set; }

    // Сам файл тут не віддаємо — тілько те, що показати на фронті.
    public bool HasCv { get; set; }
    public string? CvFileName { get; set; }
    public long? CvSizeBytes { get; set; }
    public DateTime? CvUploadedAt { get; set; }
}

public class UpdateCandidateProfileDto
{
    public string? Headline { get; set; }
    public string? About { get; set; }
}