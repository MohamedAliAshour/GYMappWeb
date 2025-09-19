using GYMappWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GYMappWeb.Areas.Identity.Pages.Account
{
    public class UserManagementModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IList<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public bool IsDeveloper { get; set; }
        public bool IsCaptain { get; set; }

        public class UserViewModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public int? GymBranch_ID { get; set; }
            public List<string> Roles { get; set; } = new List<string>();
        }

        public async Task OnGetAsync()
        {
            // Check current user's roles
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
                IsDeveloper = currentUserRoles.Contains("Developer");
                IsCaptain = currentUserRoles.Contains("Captain");
            }

            // Get all users
            var allUsers = await _userManager.Users.ToListAsync();
            var filteredUsers = new List<ApplicationUser>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (IsDeveloper)
                {
                    // Developer sees all users
                    filteredUsers.Add(user);
                }
                else if (IsCaptain)
                {
                    // Captain sees only Users (not Developers)
                    if (roles.Contains("User") && !roles.Contains("Developer"))
                    {
                        filteredUsers.Add(user);
                    }
                }
                else
                {
                    // Regular users see only other Users (not Developers or Captains)
                    if (roles.Contains("User") && !roles.Contains("Developer") && !roles.Contains("Captain"))
                    {
                        filteredUsers.Add(user);
                    }
                }
            }

            Users = filteredUsers
                .OrderBy(u => u.UserName)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    GymBranch_ID = u.GymBranchId,
                    Roles = _userManager.GetRolesAsync(u).Result.ToList()
                }).ToList();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check current user's role to determine deletion permissions
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var targetUserRoles = await _userManager.GetRolesAsync(user);

            // Developers can delete anyone except other developers
            if (currentUserRoles.Contains("Developer"))
            {
                if (targetUserRoles.Contains("Developer"))
                {
                    return BadRequest("Cannot delete other developers");
                }
            }
            // Captains can only delete Users
            else if (currentUserRoles.Contains("Captain"))
            {
                if (!targetUserRoles.Contains("User") || targetUserRoles.Contains("Developer") || targetUserRoles.Contains("Captain"))
                {
                    return BadRequest("Can only delete regular users");
                }
            }
            // Regular users cannot delete anyone
            else
            {
                return BadRequest("Insufficient permissions");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}