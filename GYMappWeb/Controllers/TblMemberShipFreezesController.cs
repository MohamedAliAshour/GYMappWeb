using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using Microsoft.AspNetCore.Authorization;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;
using GYMappWeb.Helper;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer,User")]
    public class TblMemberShipFreezesController : Controller
    {
        private readonly ITblMemberShipFreeze _freezeService;
        private readonly GYMappWebContext _context;

        public TblMemberShipFreezesController(ITblMemberShipFreeze freezeService, GYMappWebContext context)
        {
            _freezeService = freezeService;
            _context = context;
        }

        public async Task<IActionResult> Index(UserParameters userParameters)
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var freezes = await _freezeService.GetAllFreezesAsync(userParameters, gymBranchId);
            return View(freezes);
        }

        [HttpGet]
        public async Task<IActionResult> GetFreezeRecords(int userMembershipId)
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var freezeRecords = await _freezeService.GetFreezeRecordsAsync(userMembershipId, gymBranchId);
            return Json(freezeRecords);
        }

        [HttpGet]
        public async Task<IActionResult> GetMembershipFreezeDetails(int userMembershipId)
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var details = await _freezeService.GetMembershipFreezeDetailsAsync(userMembershipId, gymBranchId);
            if (details == null)
            {
                return NotFound();
            }
            return Json(details);
        }

        public async Task<IActionResult> Create()
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var activeMemberships = await _freezeService.GetActiveMembershipsForDropdownAsync(gymBranchId);
            ViewData["UserMemberShipId"] = new SelectList(activeMemberships, "UserMemberShipId", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserMemberShipId,FreezeStartDate,FreezeEndDate,Reason")] SaveTblMemberShipFreezeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    var gymBranchId = userSession.GymBranchId ?? 1;

                    // Convert DateOnly to DateTime for the service method
                    DateTime startDateTime = model.FreezeStartDate.ToDateTime(TimeOnly.MinValue);
                    DateTime endDateTime = model.FreezeEndDate.ToDateTime(TimeOnly.MinValue);

                    // Check for date overlaps
                    bool hasOverlap = await _freezeService.HasDateOverlapAsync(
                        model.UserMemberShipId,
                        startDateTime,
                        endDateTime,
                        gymBranchId);

                    if (hasOverlap)
                    {
                        // Get language preference from cookie or default to 'en'
                        string language = Request.Cookies["preferredLanguage"] ?? "en";
                        string errorMessage = language == "ar"
                            ? "هناك تداخل في تواريخ التجميد مع فترات تجميد موجودة مسبقاً"
                            : "There is an overlap with existing freeze periods";

                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        await _freezeService.AddFreezeAsync(model, userSession?.Id, gymBranchId);
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var userSessionForView = HttpContext.Session.GetUserSession();
            var gymBranchIdForView = userSessionForView.GymBranchId ?? 1;

            var activeMemberships = await _freezeService.GetActiveMembershipsForDropdownAsync(gymBranchIdForView);
            ViewData["UserMemberShipId"] = new SelectList(activeMemberships, "UserMemberShipId", "UserName", model.UserMemberShipId);
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                await _freezeService.DeleteFreezeAsync(id, gymBranchId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting freeze: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateFreezeDates(int userMembershipId, DateTime freezeStartDate, DateTime freezeEndDate)
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            bool hasOverlap = await _freezeService.HasDateOverlapAsync(userMembershipId, freezeStartDate, freezeEndDate, gymBranchId);

            // Get language preference from cookie or default to 'en'
            string language = Request.Cookies["preferredLanguage"] ?? "en";
            string errorMessage = language == "ar"
                ? "هناك تداخل في تواريخ التجميد مع فترات تجميد موجودة مسبقاً"
                : "There is an overlap with existing freeze periods";

            return Json(new
            {
                isValid = !hasOverlap,
                errorMessage = hasOverlap ? errorMessage : ""
            });
        }
    }
}