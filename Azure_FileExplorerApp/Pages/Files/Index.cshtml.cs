using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.DTOs;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Services;
using Microsoft.AspNetCore.Authorization;

namespace Azure_FileExplorerApp.Pages.Files;

[Authorize]
public class FilesIndexModel : PageModel
{
    private readonly IFileService _fileService;
    private readonly IFolderService _folderService;

    public FilesIndexModel(IFileService fileService, IFolderService folderService)
    {
        _fileService = fileService;
        _folderService = folderService;
    }

    public int FolderId { get; set; }
    public string FolderName { get; set; }
    public IEnumerable<FileMetadataDTO> Files { get; set; }

    public async Task OnGetAsync(int folderId)
    {
        FolderId = folderId;
        Files = await _fileService.GetFilesInFolderAsync(folderId);

        // Якщо папка порожня, отримуємо її ім'я через окремий сервіс
        if (Files.Any())
        {
            FolderName = Files.FirstOrDefault()?.FolderName ?? "Невідома папка";
        }
        else
        {
            FolderName = await _folderService.GetFolderNameByIdAsync(folderId) ?? "Невідома папка";
        }
    }
}
