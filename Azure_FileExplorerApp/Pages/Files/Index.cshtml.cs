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

public class IndexModel : PageModel
{
    private readonly IFileService _fileService;

    public IndexModel(IFileService fileService)
    {
        _fileService = fileService;
    }

    public int FolderId { get; set; }
    public string FolderName { get; set; }
    public IEnumerable<FileMetadata> Files { get; set; }

    public async Task OnGetAsync(int folderId)
    {
        FolderId = folderId;
        Files = await _fileService.GetFilesInFolderAsync(folderId);
        FolderName = Files.FirstOrDefault()?.Folder?.Name ?? "Невідома папка";
    }
}
