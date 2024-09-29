using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Interfaces;

namespace Azure_FileExplorerApp.Pages.Folders;

public class FoldersCreateModel : PageModel
{
    private readonly IFolderService _folderService;

    [BindProperty]
    public string FolderName { get; set; }

    public FoldersCreateModel(IFolderService folderService)
    {
        _folderService = folderService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _folderService.CreateFolderAsync(FolderName);
        return RedirectToPage("/Folders/Index");
    }
}
