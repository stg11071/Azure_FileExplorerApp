namespace Azure_FileExplorerApp.DTOs;

public class FileMetadataDTO
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string BlobUri { get; set; }
    public long Size { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public string FolderName { get; set; }
}

