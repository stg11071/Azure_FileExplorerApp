using Azure_FileExplorerApp.DTOs;
using Azure_FileExplorerApp.Models;

namespace Azure_FileExplorerApp.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(string fileName, IEnumerable<byte> data, int? folderId = null);
    Task DeleteFileAsync(string blobUri);
    Task<FileMetadata> GetFileMetadataAsync(int id);
    Task<IEnumerable<FileMetadata>> GetAllFilesAsync();
    Task<IEnumerable<FileMetadataDTO>> GetFilesInFolderAsync(int folderId); 

}


