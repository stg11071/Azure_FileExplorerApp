using Azure_FileExplorerApp.Models;

namespace Azure_FileExplorerApp.Interfaces;

public interface IFolderService
{
    Task<Folder> CreateFolderAsync(string folderName);
    Task<IEnumerable<Folder>> GetAllFoldersAsync();
    Task<Folder> GetFolderByIdAsync(int folderId);
    Task DeleteFolderAsync(int folderId);
    Task UpdateFolderAsync(Folder folder);
    Task<string?> GetFolderNameByIdAsync(int folderId);
}

