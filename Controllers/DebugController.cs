using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portal_Academico.Models;
using System.Linq;

namespace Portal_Academico.Controllers;

[Authorize]
public class DebugController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _env;

    public DebugController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _env = env;
    }

    // GET: /Debug/WhoAmI
    public async Task<IActionResult> WhoAmI()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Content("No estás autenticado.");

        var roles = await _userManager.GetRolesAsync(user);
        var info = $"UserId: {user.Id}\nUserName: {user.UserName}\nEmail: {user.Email}\nRoles: {string.Join(", ", roles)}";
        return Content(info, "text/plain");
    }

    // GET: /Debug/AssignCoordinator
    // Only allowed in Development. Creates role if missing and assigns it to current user.
    public async Task<IActionResult> AssignCoordinator()
    {
        if (!_env.IsDevelopment()) return Forbid();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var roleName = "Coordinador";
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            var res = await _userManager.AddToRoleAsync(user, roleName);
            if (!res.Succeeded)
            {
                return Content("Error al asignar rol: " + string.Join(", ", res.Errors.Select(e => e.Description)));
            }
        }

        return Content($"Role '{roleName}' assigned to user {user.UserName}.");
    }
}
