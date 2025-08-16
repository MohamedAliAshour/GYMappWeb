using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GYMappWeb.ViewModels.TblMemberShipType;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.Helper;

namespace GYMappWeb.Controllers
{
    public class TblMembershipTypesController : Controller
    {
        private readonly ITblMembershipType _membershipTypeService;

        public TblMembershipTypesController(ITblMembershipType membershipTypeService)
        {
            _membershipTypeService = membershipTypeService;
        }

        public async Task<IActionResult> Index(UserParameters userParameters)
        {
            var membershipTypes = await _membershipTypeService.GetAllMembershipTypesAsync(userParameters);
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
                    await _membershipTypeService.AddMembershipTypeAsync(model, userSession?.Id);
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
                var viewModel = await _membershipTypeService.GetMembershipTypeDetailsAsync(id.Value);
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
                    await _membershipTypeService.UpdateMembershipTypeAsync(model, id, userSession?.Id);
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
                var hasMemberships = await _membershipTypeService.HasRelatedMembershipsAsync(id);
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
                await _membershipTypeService.DeleteMembershipTypeAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting membership type: {ex.Message}");
            }
        }
    }
}