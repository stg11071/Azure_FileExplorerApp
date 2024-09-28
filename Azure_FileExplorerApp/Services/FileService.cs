using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Azure_FileExplorerApp.Services;

public class FileService : IFileService
{
    private readonly BlobService _blobService;
    private readonly DataContext _dbContext;
    private readonly ILogger<FileService> _logger;

    public FileService(BlobService blobService, DataContext dbContext, ILogger<FileService> logger)
    {
        _blobService = blobService;
        _dbContext = dbContext;
        _logger = logger;
    }

    // завантаження файлу в папку (якщо передано folderId)
    public async Task<string> UploadFileAsync(string fileName, IEnumerable<byte> data, int? folderId = null)
    {
        try
        {
            // завантаження файлу в Blob Storage через BlobService
            var blobUrl = await _blobService.UploadFileAsync(fileName, data);

            // збереження метаданих у базі даних
            var fileMetadata = new FileMetadata
            {
                FileName = fileName,
                BlobUri = blobUrl,
                Size = data.Count(),  
                UploadedBy = "Anonymous", 
                FolderId = folderId ?? 0 // якщо папка не вказана, встановлюється значення 0
            };

            _dbContext.Files.Add(fileMetadata);
            await _dbContext.SaveChangesAsync();

            return blobUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading file {FileName}", fileName);
            throw;
        }
    }

    // видалення файлу через Blob URI
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting file with BlobUri {BlobUri}", blobUri);
            throw;
        }
    }

    // отримання метаданих файлу за ID
    public async Task<FileMetadata> GetFileMetadataAsync(int id)
    {
        try
        {
            var fileMetadata = await _dbContext.Files.FindAsync(id);
            if (fileMetadata == null)
            {
                throw new Exception($"File with ID {id} not found");
            }

            return fileMetadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting file with ID {FileId}", id);
            throw;
        }
    }

    // отримання всіх файлів
    public async Task<IEnumerable<FileMetadata>> GetAllFilesAsync()
    {
        try
        {
            return await _dbContext.Files.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all files");
            throw;
        }
    }

    // отримання всіх файлів в конкретній папці
    public async Task<IEnumerable<FileMetadata>> GetFilesInFolderAsync(int folderId)
    {
        try
        {
            return await _dbContext.Files
                .Include(f => f.Folder)
                .Where(f => f.FolderId == folderId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting files in folder with ID {FolderId}", folderId);
            throw;
        }
    }

    // створення нової папки
    public async Task<Folder> CreateFolderAsync(string folderName)
    {
        try
        {
            var folder = new Folder
            {
                Name = folderName
            };

            _dbContext.Folders.Add(folder);
            await _dbContext.SaveChangesAsync();

            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating folder {FolderName}", folderName);
            throw;
        }
    }

    // отримання всіх папок
    public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
    {
        try
        {
            return await _dbContext.Folders.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all folders");
            throw;
        }
    }


    public async Task<Folder> GetFolderByIdAsync(int folderId)
    {
        try
        {
            var folder = await _dbContext.Folders.FindAsync(folderId);
            if (folder == null)
            {
                throw new Exception($"Folder with ID {folderId} not found");
            }
            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting folder with ID {FolderId}", folderId);
            throw;
        }
    }

    // видалення папки разом з усіма файлами
    public async Task DeleteFolderAsync(int folderId)
    {
        try
        {
            var folder = await _dbContext.Folders.Include(f => f.Files).FirstOrDefaultAsync(f => f.Id == folderId);
            if (folder == null)
            {
                throw new Exception($"Folder with ID {folderId} not found");
            }

            // видалення всіх файлів, що належать папці
            foreach (var file in folder.Files)
            {
                await _blobService.DeleteFileAsync(file.FileName);
                _dbContext.Files.Remove(file);
            }

            // видалення папки
            _dbContext.Folders.Remove(folder);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting folder with ID {FolderId}", folderId);
            throw;
        }
    }

    public async Task UpdateFolderAsync(Folder folder)
    {
        _dbContext.Folders.Update(folder);
        await _dbContext.SaveChangesAsync();
    }
}



