using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Data;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Azure_FileExplorerApp.Pages.Files;

[Authorize]
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

