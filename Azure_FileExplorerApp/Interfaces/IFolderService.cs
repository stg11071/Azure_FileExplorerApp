using Azure_FileExplorerApp.DTOs;
using Azure_FileExplorerApp.Models;

namespace Azure_FileExplorerApp.Interfaces;

public interface IFolderService
{
    Task<Folder> CreateFolderAsync(Folder folder);
    Task<IEnumerable<FolderDTO>> GetAllFoldersAsync();
    Task<Folder> GetFolderByIdAsync(int folderId);
    Task DeleteFolderAsync(int folderId);
    Task UpdateFolderAsync(Folder folder);
    Task<string?> GetFolderNameByIdAsync(int folderId);
}

