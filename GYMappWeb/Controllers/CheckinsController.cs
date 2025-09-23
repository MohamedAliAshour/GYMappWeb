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
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var checkins = await _checkinService.GetWithPaginations(userParameters, gymBranchId);
            return View(checkins);
        }

        // GET: Checkins/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var checkin = await _checkinService.GetCheckinDetailsAsync(id.Value, gymBranchId);
            if (checkin == null)
            {
                return NotFound();
            }

            return View(checkin);
        }

        // GET: Checkins/Create
        public async Task<IActionResult> Create()
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            await PopulateViewData(gymBranchId);
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
                    var gymBranchId = userSession.GymBranchId ?? 1;

                    await _checkinService.Add(checkin, userSession?.Id, gymBranchId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            var userSessionForView = HttpContext.Session.GetUserSession();
            var gymBranchIdForView = userSessionForView.GymBranchId ?? 1;

            await PopulateViewData(gymBranchIdForView);
            return View(checkin);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWithInvitations(SaveCheckinViewModel model, List<InvitedUserRequest> InvitedUsers)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

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

        [HttpGet]
        public async Task<IActionResult> CheckPhoneExists(string phone)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                var exists = await _checkinService.CheckPhoneExistsAsync(phone, gymBranchId);
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

            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var checkin = _checkinService.GetDetailsById(id.Value, gymBranchId);
            if (checkin == null)
            {
                return NotFound();
            }

            await PopulateViewData(gymBranchId);
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

        [HttpGet]
        public async Task<IActionResult> IsUserCheckedIn(int userId)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                var isCheckedIn = await _checkinService.IsUserCheckedInAsync(userId, gymBranchId);
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
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                var user = await _checkinService.SearchUserByCodeAsync(code, gymBranchId);
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
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                var user = await _checkinService.SearchUserByPhoneAsync(phone, gymBranchId);
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
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                await _checkinService.Delete(id, gymBranchId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting checkin: {ex.Message}");
            }
        }

        private async Task PopulateViewData(int gymBranchId)
        {
            ViewData["GymBranchId"] = new SelectList(await _checkinService.GetGymBranchesSelectList(), "Value", "Text");
            ViewData["UserId"] = new SelectList(await _checkinService.GetUsersSelectList(gymBranchId), "Value", "Text");
        }
    }
}