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

namespace Azure_FileExplorerApp.Pages.Files;
public class FilesUploadModel : PageModel
{
    private readonly IFileService _fileService;

    [BindProperty]
    public IFormFile File { get; set; }

    public int FolderId { get; set; }

    public FilesUploadModel(IFileService fileService)
    {
        _fileService = fileService;
    }

    public void OnGet(int folderId)
    {
        FolderId = folderId;
    }

    public async Task<IActionResult> OnPostAsync(int folderId)
    {
        if (File == null || File.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Будь ласка, оберіть файл.");
            return Page();
        }

        using var ms = new MemoryStream();
        await File.CopyToAsync(ms);
        var fileBytes = ms.ToArray();

        await _fileService.UploadFileAsync(File.FileName, fileBytes, folderId);

        return RedirectToPage("/Files/Index", new { folderId });
    }
}
