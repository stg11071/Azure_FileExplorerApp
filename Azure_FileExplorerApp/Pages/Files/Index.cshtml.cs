using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.DTOs;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public IEnumerable<FolderDTO> SubFolders { get; set; }

    public async Task<IActionResult> OnGetAsync(int folderId)
    {
        FolderId = folderId;

        var folder = await _folderService.GetFolderByIdAsync(folderId);

        if (folder == null)
            return NotFound();

        FolderName = folder.Name;
        Files = await _fileService.GetFilesInFolderAsync(folderId);

        var folders = await _folderService.GetAllFoldersAsync();
        SubFolders = folders.Where(f => f.ParentFolderId == folderId);

        return Page();
    }
}
