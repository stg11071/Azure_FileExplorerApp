using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.Interfaces;

namespace Azure_FileExplorerApp.Pages.Folders;

public class FoldersEditModel : PageModel
{
    private readonly IFileService _fileService;

    public FoldersEditModel(IFileService fileService)
    {
        _fileService = fileService;
    }

    [BindProperty]
    public string FolderName { get; set; }

    public int FolderId { get; set; }

    // Отримуємо назву папки при GET-запиті
    public async Task<IActionResult> OnGetAsync(int folderId)
    {
        var folder = await _fileService.GetFolderByIdAsync(folderId);
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

        var folder = await _fileService.GetFolderByIdAsync(folderId);
        if (folder == null)
        {
            return NotFound();
        }

        folder.Name = FolderName;
        await _fileService.UpdateFolderAsync(folder);

        return RedirectToPage("/Folders/Index");
    }
}
