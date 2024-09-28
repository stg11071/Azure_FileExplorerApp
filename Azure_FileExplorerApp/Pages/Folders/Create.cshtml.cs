using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.Interfaces;

namespace Azure_FileExplorerApp.Pages.Folders;

public class FoldersCreateModel : PageModel
{
    private readonly IFileService _fileService;

    [BindProperty]
    public string FolderName { get; set; }

    public FoldersCreateModel(IFileService fileService)
    {
        _fileService = fileService;
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

        await _fileService.CreateFolderAsync(FolderName);
        return RedirectToPage("/Folders/Index");
    }
}
