namespace JobPortal.Application.Interfaces.Infrastructure;

public interface IFileStorage
{
    Task UploadAsync(Stream content, string blobPath, string contentType, CancellationToken ct);
    Task<Stream?> DownloadAsync(string blobPath, CancellationToken ct);
    Task TryDeleteAsync(string blobPath);
    Task CopyAsync(string sourceBlobPath, string targetBlobPath, CancellationToken ct);
}