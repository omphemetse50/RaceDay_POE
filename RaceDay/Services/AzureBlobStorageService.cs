using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _configuration;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
        var connectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("ConnectionStrings:AzureBlobStorage is not configured.");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(string containerName, string fileName, Stream content, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        });

        var url = blobClient.Uri.ToString();

        // Fix for Docker: Replace internal Azurite hostname with localhost for the browser
        var publicBaseUrl = _configuration["BlobStorage:PublicBaseUrl"];
        if (!string.IsNullOrEmpty(publicBaseUrl))
        {
            var uri = new Uri(url);
            url = $"{publicBaseUrl.TrimEnd('/')}{uri.AbsolutePath}";
        }

        return url;
    }
}