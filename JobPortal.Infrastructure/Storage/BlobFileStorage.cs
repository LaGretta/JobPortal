using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using JobPortal.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Options;

namespace JobPortal.Infrastructure.Storage;

public class BlobFileStorage : IFileStorage
{
    // Клієнт одного контейнера. Створюється раз — він потокобезпечний.
    private readonly BlobContainerClient _container;

    public BlobFileStorage(IOptions<BlobStorageSettings> options)
    {
        var settings = options.Value;                                         // розгортаємо IOptions в сам обʼєкт
        var service = new BlobServiceClient(settings.ConnectionString);        // підключення до акаунта сховища
        _container = service.GetBlobContainerClient(settings.ContainerName);   // звужуємось до "resumes"
    }

    public async Task UploadAsync(Stream content, string blobPath, string contentType, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(blobPath);   

        await blob.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            },
            ct);
    }

    public async Task<Stream> DownloadAsync(string blobPath, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(blobPath);
        
        return await blob.OpenReadAsync(cancellationToken: ct);
    }

    public async Task DeleteAsync(string blobPath, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(blobPath);
        
        await blob.DeleteIfExistsAsync(cancellationToken: ct);
    }

    public async Task CopyAsync(string sourceBlobPath, string targetBlobPath, CancellationToken ct)
    {
        var source = _container.GetBlobClient(sourceBlobPath);
        var target = _container.GetBlobClient(targetBlobPath);

       
        var sasUri = source.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(5));

        var copy = await target.StartCopyFromUriAsync(sasUri, cancellationToken: ct);
        
        await copy.WaitForCompletionAsync(ct);
    }
}