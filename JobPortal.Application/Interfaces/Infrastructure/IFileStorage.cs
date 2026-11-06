namespace JobPortal.Application.Interfaces.Infrastructure;

public interface IFileStorage
{
    // blobPath — повний шлях у сховищі, напр. "cv/{userId}/{guid}.pdf"
    Task UploadAsync(Stream content, string blobPath, string contentType, CancellationToken ct);

    Task<Stream> DownloadAsync(string blobPath, CancellationToken ct);

    Task DeleteAsync(string blobPath, CancellationToken ct);

    // Копія на стороні Azure, без качання до себе й назад.
    // Це для знімка CV на момент відгуку.
    Task CopyAsync(string sourceBlobPath, string targetBlobPath, CancellationToken ct);
}