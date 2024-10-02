namespace Azure_FileExplorerApp.DTOs;

public class UserViewModel
{
    public string Id { get; set; }
    public string Email { get; set; }
    public IList<string> Roles { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}
