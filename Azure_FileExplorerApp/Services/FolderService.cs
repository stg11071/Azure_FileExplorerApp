using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
    public async Task<Folder> CreateFolderAsync(string folderName)
    {
        try
        {
            // перевірка, чи існує вже папка з таким ім'ям
            var existingFolder = await _dbContext.Folders
                .FirstOrDefaultAsync(f => f.Name == folderName);

            if (existingFolder != null)
            {
                _logger.LogWarning("Папка з ім'ям {folderName} вже існує", folderName);
                return null; // повертаємо існуючу папку
            }

            var createdByUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var folder = new Folder
            {
                Name = folderName,
                CreatedByUserId = createdByUserId
            };
            _dbContext.Folders.Add(folder);
            await _dbContext.SaveChangesAsync();

            // очищення кешу для всіх папок
            await _cacheService.RemoveCacheData("all_folders");

            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні нової папки з ім'ям {folderName}", folderName);
            throw; // повторно кидаємо виключення для глобальної обробки
        }
    }

    // отримання всіх папок
    public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
    {
        try
        {
            var cacheKey = "all_folders";
            
            // перевіряємо наявність папок у кеші
            var cachedFolders = await _cacheService.GetCacheData<IEnumerable<Folder>>(cacheKey);
            if (cachedFolders != null)
            {
                _logger.LogInformation("Отримано папки з кешу");
                return cachedFolders;
            }

            // якщо папок немає у кеші, отримуємо їх з бази даних
            var folders = await _dbContext.Folders.ToListAsync();

            // зберігаємо папки у кеші
            await _cacheService.AddCacheData(cacheKey, folders);

            return folders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при отриманні всіх папок");
            throw;
        }
    }

    // отримання папки за ID
    public async Task<Folder> GetFolderByIdAsync(int folderId)
    {
        try
        {
            var cacheKey = $"folder_{folderId}";

            // перевіряємо наявність папки в кеші
            var cachedFolder = await _cacheService.GetCacheData<Folder>(cacheKey);
            if (cachedFolder != null)
            {
                _logger.LogInformation("Отримано папку з кешу");
                return cachedFolder;
            }

            // отримуємо папку з бази даних
            var folder = await _dbContext.Folders.FindAsync(folderId);
            if (folder == null)
            {
                _logger.LogWarning($"Папку з ID {folderId} не знайдено");
                return null; // або кидаємо виняток, якщо це критично
            }

            // зберігаємо папку в кеші
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
            var folder = await _dbContext.Folders
                .Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.Id == folderId);
            if (folder == null)
            {
                _logger.LogWarning($"Папку з ID {folderId} не знайдено");
                return; // або кидаємо виняток, якщо це критично
            }

            // видаляємо папку і всі файли в ній
            _dbContext.Folders.Remove(folder);
            await _dbContext.SaveChangesAsync();

            // очищуємо кеш для цієї папки
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


