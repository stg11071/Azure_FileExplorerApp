namespace Azure_FileExplorerApp.Models;

public class FileMetadata
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string BlobUri { get; set; }
    public long Size { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string CreatedByUserId { get; set; }

    public int FolderId { get; set; } 
    public Folder Folder { get; set; } 
}
