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
            var offers = await _offerService.GetAllOffersAsync(userParameters);
            return View(offers);
        }

        public IActionResult Create()
        {
            SetupOfferFormViewData();
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
                    await _offerService.AddOfferAsync(model, userSession?.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            SetupOfferFormViewData(model.MemberShipTypesId);
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
                var viewModel = await _offerService.GetOfferDetailsByIdAsync(id.Value);
                SetupOfferFormViewData(viewModel.MemberShipTypesId);
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
                    await _offerService.UpdateOfferAsync(model, id, userSession?.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            SetupOfferFormViewData(model.MemberShipTypesId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> HasRelatedMemberships(int id)
        {
            try
            {
                var hasMemberships = await _offerService.HasRelatedMembershipsAsync(id);
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
                await _offerService.DeleteOfferAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting offer: {ex.Message}");
            }
        }

        private void SetupOfferFormViewData(int? selectedMembershipTypeId = null)
        {
            ViewData["MemberShipTypesId"] = new SelectList(
                _context.TblMembershipTypes,
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
                await _offerService.ToggleOfferStatusAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error toggling offer status: {ex.Message}");
            }
        }
    }
}