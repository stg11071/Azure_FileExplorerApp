using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Azure_FileExplorerApp.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ManageRolesModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ManageRolesModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public string SelectedUserId { get; set; }

    [BindProperty]
    public string SelectedRole { get; set; }

    public List<SelectListItem> Users { get; set; }
    public List<SelectListItem> Roles { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // отримання всіх користувачів та ролей
        Users = _userManager.Users.Select(u => new SelectListItem { Value = u.Id, Text = u.Email }).ToList();
        Roles = _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(SelectedUserId) || string.IsNullOrEmpty(SelectedRole))
        {
            ModelState.AddModelError(string.Empty, "Користувач або роль не вибрані");
            return Page();
        }

        var user = await _userManager.FindByIdAsync(SelectedUserId);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Користувача не знайдено");
            return Page();
        }

        if (!await _roleManager.RoleExistsAsync(SelectedRole))
        {
            ModelState.AddModelError(string.Empty, "Роль не існує");
            return Page();
        }

        var result = await _userManager.AddToRoleAsync(user, SelectedRole);

        if (result.Succeeded)
        {
            return RedirectToPage("/Admin/ManageRoles");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}

