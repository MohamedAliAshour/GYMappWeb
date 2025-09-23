using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GYMappWeb.ViewModels.TblMemberShipType;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.Helper;
using Microsoft.AspNetCore.Authorization;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer,User")]
    public class TblMembershipTypesController : Controller
    {
        private readonly ITblMembershipType _membershipTypeService;

        public TblMembershipTypesController(ITblMembershipType membershipTypeService)
        {
            _membershipTypeService = membershipTypeService;
        }

        public async Task<IActionResult> Index(UserParameters userParameters)
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var membershipTypes = await _membershipTypeService.GetAllMembershipTypesAsync(userParameters, gymBranchId);
            return View(membershipTypes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,MembershipDuration,Price,invitationCount,Description,FreezeCount,TotalFreezeDays")] SaveTblMemberShipTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    var gymBranchId = userSession.GymBranchId ?? 1;

                    await _membershipTypeService.AddMembershipTypeAsync(model, userSession?.Id, gymBranchId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                var viewModel = await _membershipTypeService.GetMembershipTypeDetailsAsync(id.Value, gymBranchId);
                return View(viewModel);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberShipTypesId,Name,MembershipDuration,Price,invitationCount,Description,FreezeCount,TotalFreezeDays")] SaveTblMemberShipTypeViewModel model)
        {
            if (id != model.MemberShipTypesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    var gymBranchId = userSession.GymBranchId ?? 1;

                    await _membershipTypeService.UpdateMembershipTypeAsync(model, id, userSession?.Id, gymBranchId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> HasRelatedMemberships(int id)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                var hasMemberships = await _membershipTypeService.HasRelatedMembershipsAsync(id, gymBranchId);
                return Json(hasMemberships);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error checking related memberships: {ex.Message}");
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

                await _membershipTypeService.DeleteMembershipTypeAsync(id, gymBranchId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting membership type: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                await _membershipTypeService.ToggleMembershipTypeStatusAsync(id, gymBranchId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error toggling membership type status: {ex.Message}");
            }
        }
    }
}