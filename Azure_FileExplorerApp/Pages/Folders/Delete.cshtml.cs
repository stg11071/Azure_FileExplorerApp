using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.Interfaces;

namespace Azure_FileExplorerApp.Pages.Folders;

public class FoldersDeleteModel : PageModel
{
    private readonly IFileService _fileService;

    public FoldersDeleteModel(IFileService fileService)
    {
        _fileService = fileService;
    }

    public Folder Folder { get; set; }

    public async Task<IActionResult> OnGetAsync(int folderId)
    {
        Folder = await _fileService.GetFolderByIdAsync(folderId);
        if (Folder == null)
        {
            return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int folderId)
    {
        await _fileService.DeleteFolderAsync(folderId);
        return RedirectToPage("/Folders/Index");
    }
}
