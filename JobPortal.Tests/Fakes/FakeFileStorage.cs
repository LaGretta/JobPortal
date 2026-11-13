using System.Collections.Concurrent;
using JobPortal.Application.Interfaces.Infrastructure;

namespace JobPortal.Tests.Fakes;


public class FakeFileStorage : IFileStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _files = new();
    
    private int _deleteAttempts;

    public int DeleteAttempts => _deleteAttempts;

    public int Count => _files.Count;

    public IReadOnlyList<string> Paths => _files.Keys.OrderBy(x => x).ToList();

    public byte[]? Read(string blobPath)
        => _files.TryGetValue(blobPath, out var bytes) ? bytes : null;

    public async Task UploadAsync(Stream content, string blobPath, string contentType, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms, ct);
        
        _files[blobPath] = ms.ToArray();
    }

    public Task<Stream?> DownloadAsync(string blobPath, CancellationToken ct)
        => Task.FromResult(_files.TryGetValue(blobPath, out var bytes)
            ? (Stream)new MemoryStream(bytes)
            : null);

    public Task TryDeleteAsync(string blobPath)
    {
        Interlocked.Increment(ref _deleteAttempts);
        _files.TryRemove(blobPath, out _);

        return Task.CompletedTask;
    }

    public Task CopyAsync(string sourceBlobPath, string targetBlobPath, CancellationToken ct)
    {
        if (!_files.TryGetValue(sourceBlobPath, out var bytes))
            throw new FileNotFoundException(sourceBlobPath);
        
        _files[targetBlobPath] = bytes.ToArray();

        return Task.CompletedTask;
    }
}