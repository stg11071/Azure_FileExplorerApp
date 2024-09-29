using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.Interfaces;

namespace Azure_FileExplorerApp.Pages.Folders;

public class FoldersDeleteModel : PageModel
{
    private readonly IFolderService _folderService;

    public FoldersDeleteModel(IFolderService folderService)
    {
        _folderService = folderService;
    }

    public Folder Folder { get; set; }

    public async Task<IActionResult> OnGetAsync(int folderId)
    {
        Folder = await _folderService.GetFolderByIdAsync(folderId);
        if (Folder == null)
        {
            return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int folderId)
    {
        await _folderService.DeleteFolderAsync(folderId);
        return RedirectToPage("/Folders/Index");
    }
}
