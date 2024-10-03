using Azure_FileExplorerApp.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Azure_FileExplorerApp.Pages.Folders;

[Authorize]
public class FoldersIndexModel : PageModel
{
    private readonly IFolderService _folderService;

    public FoldersIndexModel(IFolderService folderService)
    {
        _folderService = folderService;
    }

    public IEnumerable<FolderDTO> Folders { get; set; }

    public async Task OnGetAsync()
    {
        var allFolders = await _folderService.GetAllFoldersAsync();

        Folders = allFolders.Where(f => f.ParentFolderId == null).ToList();
    }
}

