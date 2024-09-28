using Azure_FileExplorerApp.Models;

namespace Azure_FileExplorerApp.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(string fileName, IEnumerable<byte> data, int? folderId = null); // завантаження файлу у вказану папку (якщо є)
    Task DeleteFileAsync(string blobUri);           // видалення файлу через Blob URI
    Task<FileMetadata> GetFileMetadataAsync(int id); // отримання метаданих файлу
    Task<IEnumerable<FileMetadata>> GetAllFilesAsync(); // отримання всіх файлів
    Task<IEnumerable<FileMetadata>> GetFilesInFolderAsync(int folderId); // отримання файлів у певній папці
    Task<Folder> CreateFolderAsync(string folderName); // створення нової папки
    Task<IEnumerable<Folder>> GetAllFoldersAsync(); // отримання всіх папок
    Task<Folder> GetFolderByIdAsync(int folderId); // отримання папки за ID
    Task DeleteFolderAsync(int folderId); // видалення папки разом з файлами
    Task UpdateFolderAsync(Folder folder); // оновлення інформації про папку
}


