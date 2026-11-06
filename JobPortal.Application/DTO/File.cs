namespace JobPortal.Application.DTO;

// Файл, який прислав клієнт. Application не знає про IFormFile —
// це тип з ASP.NET, а Application у тебе посилається тільки на Domain.
// Контролер перекладає IFormFile у цей DTO.
public class FileUploadDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}

// Файл який віддаємо клієнту.
public class FileDownloadDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}