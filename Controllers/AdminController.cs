
using CYBP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CYBP.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    private async Task LoadRolesAsync(string? selected = null)
    {
        ViewBag.Roles = new SelectList(
            await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(r => r)
                .ToListAsync(),
            selected);
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.UserName)
            .ToListAsync();

        var result = new List<AdminUserListItem>();

        foreach (var user in users)
        {
            result.Add(new AdminUserListItem
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                IsActive = user.IsActive,
                Role = string.Join(", ",
                    await _userManager.GetRolesAsync(user))
            });
        }

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadRolesAsync("Czytelnik");
        return View(new AdminCreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AdminCreateUserViewModel model)
    {
        if (!await _roleManager.RoleExistsAsync(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Nieznana rola.");

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(model.Role);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            MustChangePassword = true,
            PasswordChangedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(
            user, model.Password);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError("", error.Description);

            await LoadRolesAsync(model.Role);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user, model.Role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
                ModelState.AddModelError("", error.Description);

            await LoadRolesAsync(model.Role);
            return View(model);
        }

        TempData["Message"] = "Utworzono konto użytkownika.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var model = new AdminEditUserViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? "Czytelnik"
        };

        await LoadRolesAsync(model.Role);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        AdminEditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null) return NotFound();

        if (!await _roleManager.RoleExistsAsync(model.Role))
            ModelState.AddModelError(nameof(model.Role), "Nieznana rola.");

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(model.Role);
            return View(model);
        }

        var normalizedName = _userManager.NormalizeName(model.UserName.Trim());
        var nameOwner = await _userManager.FindByNameAsync(model.UserName.Trim());

        if (nameOwner is not null && nameOwner.Id != user.Id)
            ModelState.AddModelError(nameof(model.UserName),
                "Ten login jest już zajęty.");

        var emailOwner = await _userManager.FindByEmailAsync(model.Email.Trim());

        if (emailOwner is not null && emailOwner.Id != user.Id)
            ModelState.AddModelError(nameof(model.Email),
                "Ten adres e-mail jest już używany.");

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(model.Role);
            return View(model);
        }

        var oldRoles = await _userManager.GetRolesAsync(user);

        // Nie pozwalamy odebrać roli ostatniemu administratorowi.
        if (oldRoles.Contains("Administrator") &&
            model.Role != "Administrator")
        {
            var adminCount = await _userManager.GetUsersInRoleAsync(
                "Administrator");

            if (adminCount.Count <= 1)
            {
                ModelState.AddModelError(nameof(model.Role),
                    "Nie można odebrać roli ostatniemu administratorowi.");
                await LoadRolesAsync(model.Role);
                return View(model);
            }
        }

        user.UserName = model.UserName.Trim();
        user.Email = model.Email.Trim();

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError("", error.Description);

            await LoadRolesAsync(model.Role);
            return View(model);
        }

        var removeResult = await _userManager.RemoveFromRolesAsync(
            user, oldRoles);

        if (!removeResult.Succeeded)
        {
            TempData["Message"] = "Dane zapisano, ale nie udało się zmienić roli.";
            return RedirectToAction(nameof(Index));
        }

        var addResult = await _userManager.AddToRoleAsync(user, model.Role);

        if (!addResult.Succeeded)
        {
            TempData["Message"] = "Dane zapisano, ale nie udało się przypisać nowej roli.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Message"] = "Zaktualizowano konto użytkownika.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Message"] = "Nie możesz zablokować własnego konta.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;

        // LockoutEnd blokuje logowanie w mech Identity.
        var lockoutResult = await _userManager.SetLockoutEndDateAsync(
            user,
            user.IsActive ? null : DateTimeOffset.MaxValue);

        if (!lockoutResult.Succeeded)
        {
            TempData["Message"] = "Nie udało się zmienić blokady konta.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.UpdateSecurityStampAsync(user);

        TempData["Message"] = user.IsActive
            ? "Odblokowano konto."
            : "Zablokowano konto.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Message"] = "Nie możesz usunąć własnego konta.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains("Administrator"))
        {
            var admins = await _userManager.GetUsersInRoleAsync(
                "Administrator");

            if (admins.Count <= 1)
            {
                TempData["Message"] =
                    "Nie można usunąć ostatniego administratora.";
                return RedirectToAction(nameof(Index));
            }
        }

        var result = await _userManager.DeleteAsync(user);

        TempData["Message"] = result.Succeeded
            ? "Usunięto konto użytkownika."
            : string.Join("; ", result.Errors.Select(e => e.Description));

        return RedirectToAction(nameof(Index));
    }
}

public class AdminUserListItem
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
}

