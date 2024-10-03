using Azure_FileExplorerApp.Models;

namespace Azure_FileExplorerApp.DTOs;

public class FolderDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CreatedByUserId { get; set; }

    public int? ParentFolderId { get; set; }

    public List<FileMetadataDTO> Files { get; set; }
}
