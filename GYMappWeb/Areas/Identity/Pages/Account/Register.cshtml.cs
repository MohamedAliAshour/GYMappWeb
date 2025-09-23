// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using GYMappWeb.Areas.Identity.Data;

namespace GYMappWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GYMappWebContext _context; // Add this for accessing gym branches

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            GYMappWebContext context) // Add ApplicationDbContext parameter
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context; // Initialize the context
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public SelectList GymBranches { get; set; } // Add this property

        public class InputModel
        {
            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [Display(Name = "User Name")]
            public string UserName { get; set; } = null!;

            [Display(Name = "Gym Branch")]
            public int? GymBranchId { get; set; } // Add this property

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);

            // Always populate dropdown for Developers
            if (User.IsInRole("Developer"))
            {
                ViewData["GymBranches"] = new SelectList(_context.GymBranches.OrderBy(g => g.GymName), "GymBranchId", "GymName");
            }

            // Store current user's branch ID for Captains/Users using ViewData
            if (currentUser != null && (User.IsInRole("Captain") || User.IsInRole("User")))
            {
                ViewData["CurrentUserBranchId"] = currentUser.GymBranchId;
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                var currentUser = await _userManager.GetUserAsync(User);

                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.UserName, CancellationToken.None);

                user.IsActive = true;

                // Set GymBranchId based on user role
                if (User.IsInRole("Developer"))
                {
                    // Developer can choose any branch
                    if (Input.GymBranchId.HasValue)
                    {
                        user.GymBranchId = Input.GymBranchId.Value;
                    }
                }
                else if (User.IsInRole("Captain") || User.IsInRole("User"))
                {
                    // Captain and User get the same branch as the current user
                    user.GymBranchId = currentUser.GymBranchId;
                }

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Assign User role
                    await _userManager.AddToRoleAsync(user, "User");

                    // Sign in the user (since RequireConfirmedAccount = false)
                    //await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect to UserManagement page
                    return RedirectToPage("/Account/UserManagement");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Repopulate dropdown if needed for Developers
            if (User.IsInRole("Developer"))
            {
                ViewData["GymBranches"] = new SelectList(_context.GymBranches.OrderBy(g => g.GymName), "GymBranchId", "GymName");
            }

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}