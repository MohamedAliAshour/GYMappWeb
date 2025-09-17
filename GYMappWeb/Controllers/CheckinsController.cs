using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.ViewModels.Checkin;
using GYMappWeb.Helper;
using Microsoft.AspNetCore.Mvc.Rendering;
using GYMappWeb.Models;
using System.Security.Claims;
using GYMappWeb.ViewModels.InvitedUserRequest;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer,User")]
    public class CheckinsController : Controller
    {
        private readonly ICheckin _checkinService;

        public CheckinsController(ICheckin checkinService)
        {
            _checkinService = checkinService;
        }

        // GET: Checkins
        public async Task<IActionResult> Index(UserParameters userParameters)
        {
            var checkins = await _checkinService.GetWithPaginations(userParameters);
            return View(checkins);
        }

        // GET: Checkins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkin = await _checkinService.GetCheckinDetailsAsync(id.Value);
            if (checkin == null)
            {
                return NotFound();
            }

            return View(checkin);
        }

        // GET: Checkins/Create
        public async Task<IActionResult> Create()
        {
            await PopulateViewData();
            var model = new SaveCheckinViewModel
            {
                CheckinDate = DateTime.Now
            };
            return View(model);
        }

        // POST: Checkins/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveCheckinViewModel checkin)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    await _checkinService.Add(checkin, userSession?.Id, userSession.GymBranchId ?? 1);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await PopulateViewData();
            return View(checkin);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWithInvitations(SaveCheckinViewModel model, List<InvitedUserRequest> InvitedUsers)
        {
            try
            {
                // Get gym branch ID from current user context or other method
                int gymBranchId = GetCurrentGymBranchId(); // Implement this method

                var result = await _checkinService.CreateCheckinWithInvitationsAsync(
                    model, InvitedUsers, User.FindFirstValue(ClaimTypes.NameIdentifier), gymBranchId);

                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return BadRequest("Failed to create checkin with invitations");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private int GetCurrentGymBranchId()
        {
            // Use the same session approach as your Create method
            var userSession = HttpContext.Session.GetUserSession();

            if (userSession?.GymBranchId != null)
            {
                return userSession.GymBranchId.Value;
            }

            // Fallback: If session doesn't have gym branch ID, try to get it from user claims
            var gymBranchIdClaim = User.FindFirst("GymBranchId");
            if (gymBranchIdClaim != null && int.TryParse(gymBranchIdClaim.Value, out int gymBranchId))
            {
                return gymBranchId;
            }

            // Final fallback: Return a default value
            return 1; // Default gym branch ID - change this to your actual default
        }

        [HttpGet]
        public async Task<IActionResult> CheckPhoneExists(string phone)
        {
            try
            {
                var exists = await _checkinService.CheckPhoneExistsAsync(phone);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // GET: Checkins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkin = _checkinService.GetDetailsById(id.Value);
            if (checkin == null)
            {
                return NotFound();
            }

            await PopulateViewData();
            var saveViewModel = new SaveCheckinViewModel
            {
                CheckinId = checkin.CheckinId,
                CheckinDate = checkin.CheckinDate,
                UserId = checkin.UserId,
                GymBranchId = checkin.GymBranchId,
                CreatedBy = checkin.CreatedBy
            };

            return View(saveViewModel);
        }

        // POST: Checkins/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, SaveCheckinViewModel checkinViewModel)
        //{
        //    if (id != checkinViewModel.CheckinId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var userSession = HttpContext.Session.GetUserSession();
        //            await _checkinService.Update(checkinViewModel, id, userSession?.Id);
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception ex)
        //        {
        //            ModelState.AddModelError(string.Empty, ex.Message);
        //        }
        //    }

        //    await PopulateViewData();
        //    return View(checkinViewModel);
        //}

        // Add these methods to CheckinsController.cs


        [HttpGet]
        public async Task<IActionResult> IsUserCheckedIn(int userId)
        {
            try
            {
                var isCheckedIn = await _checkinService.IsUserCheckedInAsync(userId);
                return Ok(isCheckedIn);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error checking user check-in status: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchUserByCode(int code)
        {
            try
            {
                var user = await _checkinService.SearchUserByCodeAsync(code);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchUserByPhone(string phone)
        {
            try
            {
                var user = await _checkinService.SearchUserByPhoneAsync(phone);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _checkinService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting checkin: {ex.Message}");
            }
        }

        private async Task PopulateViewData()
        {
            ViewData["GymBranchId"] = new SelectList(await _checkinService.GetGymBranchesSelectList(), "Value", "Text");
            ViewData["UserId"] = new SelectList(await _checkinService.GetUsersSelectList(), "Value", "Text");
        }
    }
}