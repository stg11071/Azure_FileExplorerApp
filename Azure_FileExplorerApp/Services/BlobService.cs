using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;

namespace Azure_FileExplorerApp.Services;

public class BlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<BlobService> _logger;
    private readonly string _blobContainerName = "files";

    public BlobService(string connectionString, ILogger<BlobService> logger)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
        _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
        _blobContainerClient.CreateIfNotExists(PublicAccessType.Blob); // відкрити публічний доступ для читання до контейнера
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string fileName, IEnumerable<byte> data)
    {
        try
        {
            // генерація унікального імені для файлу
            var extension = Path.GetExtension(fileName);
            var name = Path.GetFileNameWithoutExtension(fileName);
            var uniqueBlobName = $"{name}_{Guid.NewGuid()}{extension}";  // додавання GUID до імені файлу

            var blobClient = _blobContainerClient.GetBlobClient(uniqueBlobName);

            // завантаження файлу в Blob Storage через MemoryStream
            using var ms = new MemoryStream(data.ToArray());
            await blobClient.UploadAsync(ms, true);

            // повертаємо URL до завантаженого блобу
            return GetBlobUrl(uniqueBlobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading blob {FileName}", fileName);
            throw;
        }
    }

    // отримання URL завантаженого блобу
    public string GetBlobUrl(string blobName)
    {
        try
        {
            return _blobContainerClient.GetBlobClient(blobName).Uri.ToString();
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError($"Request to Azure Blob Storage failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while getting the Blob URL: {ex.Message}");
            throw;
        }
    }

    // видалення файлу з Blob Storage
    public async Task DeleteFileAsync(string blobName)
    {
        try
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting blob {BlobName}", blobName);
            throw;
        }
    }
}


