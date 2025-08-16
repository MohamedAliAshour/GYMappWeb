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
    [Authorize(Roles = "Captain,Developer")]
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
            var freezes = await _freezeService.GetAllFreezesAsync(userParameters);
            return View(freezes);
        }

        [HttpGet]
        public async Task<IActionResult> GetFreezeRecords(int userMembershipId)
        {
            var freezeRecords = await _freezeService.GetFreezeRecordsAsync(userMembershipId);
            return Json(freezeRecords);
        }

        [HttpGet]
        public async Task<IActionResult> GetMembershipFreezeDetails(int userMembershipId)
        {
            var details = await _freezeService.GetMembershipFreezeDetailsAsync(userMembershipId);
            if (details == null)
            {
                return NotFound();
            }
            return Json(details);
        }

        public async Task<IActionResult> Create()
        {
            var activeMemberships = await _freezeService.GetActiveMembershipsForDropdownAsync();
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
                    await _freezeService.AddFreezeAsync(model, userSession?.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var activeMemberships = await _freezeService.GetActiveMembershipsForDropdownAsync();
            ViewData["UserMemberShipId"] = new SelectList(activeMemberships, "UserMemberShipId", "UserName", model.UserMemberShipId);
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _freezeService.DeleteFreezeAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting freeze: {ex.Message}");
            }
        }
    }
}