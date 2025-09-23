using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;
using Microsoft.AspNetCore.Authorization;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer,User")]
    public class TblOffersController : Controller
    {
        private readonly ITblOffer _offerService;
        private readonly GYMappWebContext _context;

        public TblOffersController(ITblOffer offerService, GYMappWebContext context)
        {
            _offerService = offerService;
            _context = context;
        }

        public async Task<IActionResult> Index(UserParameters userParameters)
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var offers = await _offerService.GetAllOffersAsync(userParameters, gymBranchId);
            return View(offers);
        }

        public IActionResult Create()
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            SetupOfferFormViewData(gymBranchId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OffId,OfferName,DiscountPrecentage,MemberShipTypesId")] SaveTblOfferViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    var gymBranchId = userSession.GymBranchId ?? 1;

                    await _offerService.AddOfferAsync(model, userSession?.Id, gymBranchId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var userSessionForView = HttpContext.Session.GetUserSession();
            var gymBranchIdForView = userSessionForView.GymBranchId ?? 1;

            SetupOfferFormViewData(gymBranchIdForView, model.MemberShipTypesId);
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

                var viewModel = await _offerService.GetOfferDetailsByIdAsync(id.Value, gymBranchId);
                SetupOfferFormViewData(gymBranchId, viewModel.MemberShipTypesId);
                return View(viewModel);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OffId,OfferName,DiscountPrecentage,MemberShipTypesId")] SaveTblOfferViewModel model)
        {
            if (id != model.OffId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    var gymBranchId = userSession.GymBranchId ?? 1;

                    await _offerService.UpdateOfferAsync(model, id, userSession?.Id, gymBranchId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var userSessionForView = HttpContext.Session.GetUserSession();
            var gymBranchIdForView = userSessionForView.GymBranchId ?? 1;

            SetupOfferFormViewData(gymBranchIdForView, model.MemberShipTypesId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> HasRelatedMemberships(int id)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                var hasMemberships = await _offerService.HasRelatedMembershipsAsync(id, gymBranchId);
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

                await _offerService.DeleteOfferAsync(id, gymBranchId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting offer: {ex.Message}");
            }
        }

        private void SetupOfferFormViewData(int gymBranchId, int? selectedMembershipTypeId = null)
        {
            ViewData["MemberShipTypesId"] = new SelectList(
                _context.TblMembershipTypes.Where(m => m.GymBranchId == gymBranchId && m.IsActive == true), // Filter by gym branch
                "MemberShipTypesId",
                "Name",
                selectedMembershipTypeId
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;

                await _offerService.ToggleOfferStatusAsync(id, gymBranchId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error toggling offer status: {ex.Message}");
            }
        }
    }
}