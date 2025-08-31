using GYMappWeb.ViewModels.UserRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Developer")]
    public class UserRolesController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRolesController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: UserRoles
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var viewModel = new List<ManageUserRolesViewModel>();

            foreach (var user in users)
            {
                viewModel.Add(new ManageUserRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    UserRoles = await GetUserRolesViewModel(user)
                });
            }

            return View(viewModel);
        }

        // GET: UserRoles/ManageRoles/userId
        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                UserRoles = await GetUserRolesViewModel(user)
            };

            return View(model);
        }

        // POST: UserRoles/ManageRoles/userId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Get current user roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Get selected roles from model
            var selectedRoles = model.UserRoles
                .Where(x => x.Selected)
                .Select(x => x.RoleName)
                .ToList();

            // Calculate roles to add and remove
            var rolesToAdd = selectedRoles.Except(currentRoles);
            var rolesToRemove = currentRoles.Except(selectedRoles);

            // Update user roles
            var result = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove roles");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add roles");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: UserRoles/CreateRole
        public IActionResult CreateRole()
        {
            return View();
        }

        // POST: UserRoles/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole([Bind("RoleName")] UserRolesViewModel model)
        {
            if (ModelState.IsValid)
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("RoleName", "Role already exists");
                    return View(model);
                }

                var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName.Trim()));
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                AddIdentityErrors(result);
            }

            return View(model);
        }

        // POST: UserRoles/DeleteRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }

            // Check if any users are assigned to this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                TempData["Error"] = "Cannot delete role with assigned users";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to delete role";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<UserRolesViewModel>> GetUserRolesViewModel(IdentityUser user)
        {
            var userRoles = new List<UserRolesViewModel>();
            var allRoles = await _roleManager.Roles.ToListAsync();
            var userRoleNames = await _userManager.GetRolesAsync(user);

            foreach (var role in allRoles)
            {
                userRoles.Add(new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Selected = userRoleNames.Contains(role.Name)
                });
            }

            return userRoles;
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}