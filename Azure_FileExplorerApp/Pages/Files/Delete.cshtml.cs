using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure_FileExplorerApp.Models;
using Azure_FileExplorerApp.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Azure_FileExplorerApp.Pages.Files;

[Authorize]
public class FilesDeleteModel : PageModel
{
    private readonly IFileService _fileService;

    public FilesDeleteModel(IFileService fileService)
    {
        _fileService = fileService;
    }

    public FileMetadata FileMetadata { get; set; }
    public int FolderId { get; set; }

    public async Task<IActionResult> OnGetAsync(int fileId)
    {
        FileMetadata = await _fileService.GetFileMetadataAsync(fileId);
        if (FileMetadata == null)
        {
            return NotFound();
        }
        FolderId = FileMetadata.FolderId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int fileId)
    {
        FileMetadata = await _fileService.GetFileMetadataAsync(fileId);
        if (FileMetadata == null)
        {
            return NotFound();
        }

        await _fileService.DeleteFileAsync(FileMetadata.BlobUri);
        return RedirectToPage("/Files/Index", new { folderId = FileMetadata.FolderId });
    }
}
