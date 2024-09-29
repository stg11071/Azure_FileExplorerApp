using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.Interfaces;

namespace Azure_FileExplorerApp.Pages.Folders;

public class FoldersIndexModel : PageModel
{
    private readonly IFolderService _folderService;

    public FoldersIndexModel(IFolderService folderService)
    {
        _folderService = folderService;
    }

    public IEnumerable<Folder> Folders { get; set; }

    public async Task OnGetAsync()
    {
        Folders = await _folderService.GetAllFoldersAsync();
    }
}

