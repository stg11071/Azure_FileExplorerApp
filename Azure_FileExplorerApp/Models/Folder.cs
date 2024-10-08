﻿namespace Azure_FileExplorerApp.Models;

public class Folder
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CreatedByUserId { get; set; }

    public int? ParentFolderId { get; set; }

    public virtual ICollection<FileMetadata> Files { get; set; }
}

