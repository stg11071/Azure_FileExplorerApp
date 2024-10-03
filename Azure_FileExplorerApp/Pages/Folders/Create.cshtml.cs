using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Interfaces;
using Azure_FileExplorerApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace Azure_FileExplorerApp.Pages.Folders;

[Authorize]
public class FoldersCreateModel : PageModel
{
    private readonly IFolderService _folderService;


    public FoldersCreateModel(IFolderService folderService)
    {
        _folderService = folderService;
    }

    [BindProperty]
    public Folder Folder { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ParentFolderId { get; set; } // ID батьківської папки



    public async Task<IActionResult> OnGetAsync()
    {
        if (ParentFolderId.HasValue)
        {
            var parentFolder = await _folderService.GetFolderByIdAsync(ParentFolderId.Value);
            if (parentFolder == null)
            {
                return NotFound();
            }
        }
        return Page();
    }



    public async Task<IActionResult> OnPostAsync()
    {
        // прив'язка нової папки до батьківської(якщо та існує)
        if (ParentFolderId.HasValue)
        {
            Folder.ParentFolderId = ParentFolderId;
        }

        var createdFolder = await _folderService.CreateFolderAsync(Folder);

        if (createdFolder == null)
        {
            ModelState.AddModelError("", "Папка з таким ім'ям вже існує в поточній папці.");
            return Page();
        }

        // Повернення на сторінку батьківської папки після створення
        if (ParentFolderId.HasValue)
        {
            return RedirectToPage("/Files/Index", new { folderId = ParentFolderId });
        }

        return RedirectToPage("/Folders/Index");
    }
}
