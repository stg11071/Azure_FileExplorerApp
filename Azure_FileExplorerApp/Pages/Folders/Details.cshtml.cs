using Azure_FileExplorerApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Azure_FileExplorerApp.Models;

namespace Azure_FileExplorerApp.Pages.Folders;

public class DetailsModel : PageModel
{
    private readonly DataContext _context;

    public DetailsModel(DataContext context)
    {
        _context = context;
    }

    public Folder Folder { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var folder = await _context.Folders.FirstOrDefaultAsync(m => m.Id == id);
        if (folder == null)
        {
            return NotFound();
        }
        else
        {
            Folder = folder;
        }
        return Page();
    }
}

