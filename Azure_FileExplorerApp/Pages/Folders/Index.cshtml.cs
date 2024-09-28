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

public class FoldersIndexModel : PageModel
{
    private readonly IFileService _fileService;

    public FoldersIndexModel(IFileService fileService)
    {
        _fileService = fileService;
    }

    public IEnumerable<Folder> Folders { get; set; }

    public async Task OnGetAsync()
    {
        Folders = await _fileService.GetAllFoldersAsync();
    }
}

