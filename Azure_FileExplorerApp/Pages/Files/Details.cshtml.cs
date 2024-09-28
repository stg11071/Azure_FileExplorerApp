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

namespace Azure_FileExplorerApp.Pages.Files;

public class FilesDetailsModel : PageModel
{
    private readonly IFileService _fileService;

    public FilesDetailsModel(IFileService fileService)
    {
        _fileService = fileService;
    }

    public FileMetadata FileMetadata { get; set; }

    public async Task<IActionResult> OnGetAsync(int fileId)
    {
        FileMetadata = await _fileService.GetFileMetadataAsync(fileId);
        if (FileMetadata == null)
        {
            return NotFound();
        }
        return Page();
    }
}

