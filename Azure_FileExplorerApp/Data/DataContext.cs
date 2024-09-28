using Azure_FileExplorerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Azure_FileExplorerApp.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public DbSet<FileMetadata> Files { get; set; }
    public DbSet<Folder> Folders { get; set; }

}

