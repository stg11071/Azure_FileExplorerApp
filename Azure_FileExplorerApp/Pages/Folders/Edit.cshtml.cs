using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Azure_FileExplorerApp.Pages.Folders;

[Authorize]
public class FoldersEditModel : PageModel
{
    private readonly IFolderService _folderService;

    public FoldersEditModel(IFolderService folderService)
    {
        _folderService = folderService;
    }

    [BindProperty]
    public string FolderName { get; set; }

    public int FolderId { get; set; }

    // Отримуємо назву папки при GET-запиті
    public async Task<IActionResult> OnGetAsync(int folderId)
    {
        var folder = await _folderService.GetFolderByIdAsync(folderId);
        if (folder == null)
        {
            return NotFound();
        }

        FolderId = folderId;
        FolderName = folder.Name;

        return Page();
    }

    // Оновлюємо назву папки при POST-запиті
    public async Task<IActionResult> OnPostAsync(int folderId)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var folder = await _folderService.GetFolderByIdAsync(folderId);
        if (folder == null)
        {
            return NotFound();
        }

        folder.Name = FolderName;
        await _folderService.UpdateFolderAsync(folder);

        return RedirectToPage("/Folders/Index");
    }
}
