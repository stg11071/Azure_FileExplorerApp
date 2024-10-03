using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.DTOs;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Azure_FileExplorerApp.Services;

public class FolderService : IFolderService
{
    private readonly DataContext _dbContext;
    private readonly ILogger<FolderService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FolderService(DataContext dbContext, ILogger<FolderService> logger, ICacheService cacheService, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cacheService = cacheService;
        _httpContextAccessor = httpContextAccessor;
    }

    // створення нової папки
    public async Task<Folder> CreateFolderAsync(Folder folder)
    {
        try
        {
            // перевірка, чи існує вже папка з таким ім'ям (і, можливо, з тим же батьківським ID)
            var existingFolder = await _dbContext.Folders
                .FirstOrDefaultAsync(f => f.Name == folder.Name && f.ParentFolderId == folder.ParentFolderId);

            if (existingFolder != null)
            {
                _logger.LogWarning("Папка з ім'ям {folderName} вже існує", folder.Name);
                return null; // повертаємо null, оскільки папка вже існує
            }

            // отримання ID користувача, який створює папку
            var createdByUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(createdByUserId))
            {
                folder.CreatedByUserId = createdByUserId;
            }

            _dbContext.Folders.Add(folder);
            await _dbContext.SaveChangesAsync();

            // очищення кешу для всіх папок
            await _cacheService.RemoveCacheData("all_folders");

            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні нової папки з ім'ям {folderName}", folder.Name);
            throw; // проброс виключення для глобальної обробки
        }
    }


    // отримання всіх папок
    public async Task<IEnumerable<FolderDTO>> GetAllFoldersAsync()
    {
        try
        {
            var cacheKey = "all_folders";

            // перевіряємо наявність папок у кеші
            var cachedFolders = await _cacheService.GetCacheData<IEnumerable<FolderDTO>>(cacheKey);
            if (cachedFolders != null)
            {
                _logger.LogInformation("Отримано папки з кешу");
                return cachedFolders;
            }

            // якщо папок немає у кеші, отримуємо їх з бази даних
            var folders = await _dbContext.Folders
                .Include(f => f.Files) // Включаємо файли
                .ToListAsync();

            // перетворення папок в DTO
            var folderDTOs = folders.Select(f => ConvertToFolderDTO(f)).ToList();

            // зберігання папок у кеші
            await _cacheService.AddCacheData(cacheKey, folderDTOs);

            return folderDTOs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні всіх папок");
            throw;
        }
    }


    public FolderDTO ConvertToFolderDTO(Folder folder)
    {
        return new FolderDTO
        {
            Id = folder.Id,
            Name = folder.Name,
            CreatedByUserId = folder.CreatedByUserId,
            ParentFolderId = folder.ParentFolderId,
            Files = folder.Files?.Select(f => new FileMetadataDTO
            {
                Id = f.Id,
                FileName = f.FileName,
                BlobUri = f.BlobUri
            }).ToList() ?? new List<FileMetadataDTO>(), // Якщо немає файлів, повертаємо порожній список
        };
    }

    // Отримання папки за ID
    public async Task<Folder> GetFolderByIdAsync(int folderId)
    {
        try
        {
            var cacheKey = $"folder_{folderId}";

            // перевірка наявності папки в кеші
            var cachedFolder = await _cacheService.GetCacheData<Folder>(cacheKey);
            if (cachedFolder != null)
            {
                _logger.LogInformation("Отримано папку з кешу");
                return cachedFolder;
            }

            // отримання папки з бази даних
            var folder = await _dbContext.Folders
                .Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.Id == folderId);
            if (folder == null)
            {
                _logger.LogWarning($"Папку з ID {folderId} не знайдено");
                return null;
            }

            // зберігання папки в кеші
            await _cacheService.AddCacheData(cacheKey, folder);

            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні папки з ID {folderId}", folderId);
            throw;
        }
    }


    public async Task<string> GetFolderNameByIdAsync(int folderId)
    {
        var folder = await _dbContext.Folders.FindAsync(folderId);
        return folder?.Name; // повертаємо ім'я папки або null, якщо папка не знайдена
    }


    // оновлення інформації про папку
    public async Task UpdateFolderAsync(Folder folder)
    {
        try
        {
            _dbContext.Folders.Update(folder);
            await _dbContext.SaveChangesAsync();

            // очищення кешу для всіх папок
            await _cacheService.RemoveCacheData("all_folders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні папки з ID {folderId}", folder.Id);
            throw;
        }
    }

    // видалення папки разом з файлами
    public async Task DeleteFolderAsync(int folderId)
    {
        try
        {
            var folders = _dbContext.Folders
                .Include(f => f.Files);
            
            var folder = await folders.FirstOrDefaultAsync(f => f.Id == folderId);

            if (folder == null)
            {
                _logger.LogWarning($"Папку з ID {folderId} не знайдено");
                return; // або кидаємо виняток, якщо це критично
            }

            // видалення папки і всіх файлів в ній
            _dbContext.Folders.Remove(folder);
            //видалення всіх папок дочірніх папок
            var subFolders = folders.Where(f => f.ParentFolderId == folderId);
            _dbContext.Folders.RemoveRange(subFolders);

            await _dbContext.SaveChangesAsync();


            // очищення кешу для цієї папки
            await _cacheService.RemoveCacheData($"folder_{folderId}");


            // очищення кешу для всіх папок
            await _cacheService.RemoveCacheData("all_folders");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні папки з ID {folderId}", folderId);
            throw; // повторно кидаємо виключення для глобальної обробки
        }
    }
}


