using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure_FileExplorerApp.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure_FileExplorerApp.Pages.Admin;


[Authorize(Roles = "Admin")]
public class UsersModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<UserViewModel> Users { get; set; }

    [BindProperty]
    public string SelectedRole { get; set; }

    public List<SelectListItem> AllRoles { get; set; }

    public async Task OnGetAsync()
    {
        Users = new List<UserViewModel>();

        var users = _userManager.Users.ToList();
        var roles = _roleManager.Roles.ToList();

        // створення випадаючого списку для ролей
        AllRoles = roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();

        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            Users.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Roles = userRoles,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd
            });
        }
    }

    // Редагування ролі юзера
    public async Task<IActionResult> OnPostEditRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // отримуємо всі ролі користувача
        var currentRoles = await _userManager.GetRolesAsync(user);

        // видаляємо всі ролі
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Помилка при видаленні старих ролей");
            return Page();
        }

        // додаємо нову роль
        var addResult = await _userManager.AddToRoleAsync(user, SelectedRole);
        if (!addResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Помилка при додаванні нової ролі");
            return Page();
        }

        return RedirectToPage();
    }


    // Видалення юзера
    public async Task<IActionResult> OnPostDeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        await _userManager.DeleteAsync(user);
        return RedirectToPage();
    }

    // Блокування юзера
    public async Task<IActionResult> OnPostBlockUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // блокування користувача на певний час
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(1000));
        await _userManager.SetLockoutEnabledAsync(user, true);
        return RedirectToPage();
    }

    // Розблокування юзера
    public async Task<IActionResult> OnPostUnblockUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // розблоковання користувача
        await _userManager.SetLockoutEndDateAsync(user, null);
        return RedirectToPage();
    }
}


