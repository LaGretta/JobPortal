using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using JobPortal.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobPortal.Infrastructure.Storage;

public class BlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _container;
    private readonly ILogger<BlobFileStorage> _logger;

    public BlobFileStorage(IOptions<BlobStorageSettings> options, ILogger<BlobFileStorage> logger)
    {
        var settings = options.Value;
        var service = new BlobServiceClient(settings.ConnectionString);
        _container = service.GetBlobContainerClient(settings.ContainerName);
        _logger = logger;
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

    public async Task<Stream?> DownloadAsync(string blobPath, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(blobPath);

        try
        {
            return await blob.OpenReadAsync(cancellationToken: ct);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task TryDeleteAsync(string blobPath)
    {
        try
        {
            await _container.GetBlobClient(blobPath).DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            //по логах потім видно які файли осиротіли у сховищі
            _logger.LogWarning(ex, "Не вдалось видалити файл {BlobPath}", blobPath);
        }
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