using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.DTOs;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Azure_FileExplorerApp.Services;

public class FileService : IFileService
{
    private readonly BlobService _blobService;
    private readonly DataContext _dbContext;
    private readonly ILogger<FileService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FileService(BlobService blobService, DataContext dbContext, ILogger<FileService> logger, ICacheService cacheService, IHttpContextAccessor httpContextAccessor)
    {
        _blobService = blobService;
        _dbContext = dbContext;
        _logger = logger;
        _cacheService = cacheService;
        _httpContextAccessor = httpContextAccessor;
    }

    // Завантаження файлу в папку (якщо передано folderId)
    public async Task<string> UploadFileAsync(string fileName, IEnumerable<byte> data, int? folderId = null)
    {
        try
        {
            // завантаження файлу в Blob Storage через BlobService
            var blobUrl = await _blobService.UploadFileAsync(fileName, data);

            // отримання ID користувача (CreatedByUserId)
            var createdByUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // отримання Name користувача 
            var uploadedBy = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Anonymous";

            // створення метаданих файлу
            var fileMetadata = new FileMetadata
            {
                FileName = fileName,
                BlobUri = blobUrl,
                Size = data.Count(),
                UploadedBy = uploadedBy,
                CreatedByUserId = createdByUserId,
                UploadedAt = DateTime.UtcNow,
                FolderId = folderId ?? 0
            };

            // додавання метаданих файлу в базу даних
            _dbContext.Files.Add(fileMetadata);
            await _dbContext.SaveChangesAsync();

            // очищення кешу для всіх файлів та файлів у конкретній папці
            await _cacheService.RemoveCacheData("all_files");
            if (folderId.HasValue)
                await _cacheService.RemoveCacheData($"files_in_folder_{folderId}");
            
            return blobUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading file {FileName}", fileName);
            throw;
        }
    }


    // Видалення файлу через Blob URI
    public async Task DeleteFileAsync(string blobUri)
    {
        try
        {
            var fileMetadata = await _dbContext.Files.FirstOrDefaultAsync(f => f.BlobUri == blobUri);
            if (fileMetadata == null)
            {
                throw new Exception($"File with BlobUri {blobUri} not found");
            }

            // видалення файлу з Blob Storage
            await _blobService.DeleteFileAsync(fileMetadata.FileName);

            // видалення метаданих з бази даних
            _dbContext.Files.Remove(fileMetadata);
            await _dbContext.SaveChangesAsync();

            // очищення кешу для всіх файлів і файлів у конкретній папці
            await _cacheService.RemoveCacheData("all_files");
            await _cacheService.RemoveCacheData($"file_{fileMetadata.Id}");
            await _cacheService.RemoveCacheData($"files_in_folder_{fileMetadata.FolderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting file with BlobUri {BlobUri}", blobUri);
            throw;
        }
    }

    // Отримання метаданих файлу за ID
    public async Task<FileMetadata> GetFileMetadataAsync(int id)
    {
        try
        {
            var cacheKey = $"file_{id}";

            // перевірка наявність файлу в кеші
            var cachedFile = await _cacheService.GetCacheData<FileMetadata>(cacheKey);
            if (cachedFile != null)
            {
                _logger.LogInformation("Отримано метадані файлу з кешу");
                return cachedFile;
            }

            // якщо в кеші немає, отримуємо файл з бази даних
            var fileMetadata = await _dbContext.Files.FindAsync(id);
            if (fileMetadata == null)
            {
                throw new Exception($"File with ID {id} not found");
            }

            // зберігання даних в кеші
            await _cacheService.AddCacheData(cacheKey, fileMetadata);

            return fileMetadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting file with ID {FileId}", id);
            throw;
        }
    }

    // Отримання всіх файлів
    public async Task<IEnumerable<FileMetadata>> GetAllFilesAsync()
    {
        try
        {
            var cacheKey = "all_files";

            // перевіряємо наявність файлів у кеші
            var cachedFiles = await _cacheService.GetCacheData<IEnumerable<FileMetadata>>(cacheKey);
            if (cachedFiles != null)
            {
                _logger.LogInformation("Отримано всі файли з кешу");
                return cachedFiles;
            }

            // якщо файлів немає у кеші, отримуємо їх з бази даних
            var files = await _dbContext.Files.ToListAsync();

            // зберігаємо файли у кеші
            await _cacheService.AddCacheData(cacheKey, files);

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all files");
            throw;
        }
    }

    // Отримання всіх файлів у конкретній папці
    public async Task<IEnumerable<FileMetadataDTO>> GetFilesInFolderAsync(int folderId)
    {
        try
        {
            var cacheKey = $"files_in_folder_{folderId}";

            // перевірка наявністі файлів у кеші
            var cachedFiles = await _cacheService.GetCacheData<IEnumerable<FileMetadataDTO>>(cacheKey);
            if (cachedFiles != null)
            {
                _logger.LogInformation("Отримано файли з кешу для папки {FolderId}", folderId);
                return cachedFiles;
            }

            // отримання файлів з бази даних, якщо іх немає у кеші
            var files = await _dbContext.Files
                .Include(f => f.Folder)
                .Where(f => f.FolderId == folderId)
                .ToListAsync();

            // перетворення на DTO для кешування
            var filesDto = files.Select(f => new FileMetadataDTO
            {
                Id = f.Id,
                FileName = f.FileName,
                BlobUri = f.BlobUri,
                Size = f.Size,
                UploadedBy = f.UploadedBy,
                UploadedAt = f.UploadedAt,
                FolderName = f.Folder.Name,
                CreatedByUserId = f.CreatedByUserId,
});

            // зберігання у кеші
            await _cacheService.AddCacheData(cacheKey, filesDto);

            return filesDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting files in folder with ID {FolderId}", folderId);
            throw;
        }
    }

}




