using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GYMappWeb.ViewModels.TblUserMemberShip;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Helper;

namespace GYMappWeb.Controllers
{
    public class TblUserMemberShipsController : Controller
    {
        private readonly ITblUserMemberShip _membershipService;
        private readonly GYMappWebContext _context;

        public TblUserMemberShipsController(ITblUserMemberShip membershipService, GYMappWebContext context)
        {
            _membershipService = membershipService;
            _context = context;
        }

        public async Task<IActionResult> Index(UserParameters userParameters)
        {
            var memberships = await _membershipService.GetAllUserMembershipsAsync(userParameters);
            return View(memberships);
        }

        public IActionResult Create()
        {
            SetupCreateViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveTblUserMemberShipViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    await _membershipService.AddMembershipAsync(model, userSession?.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            SetupCreateViewBag();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFreezes(int id)
        {
            try
            {
                await _membershipService.DeleteFreezesAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting freezes: {ex.Message}");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _membershipService.DeleteMembershipAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting membership: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckActiveMembership(int userId)
        {
            var hasActive = await _membershipService.HasActiveMembershipAsync(userId);
            return Json(new { hasActiveMembership = hasActive });
        }

        private void SetupCreateViewBag()
        {
            // Filter active users for both userDetails and SelectList
            var activeUsers = _context.TblUsers
                .Where(u => u.IsActive == false)
                .ToList();

            var userDetails = activeUsers
                .Select(u => new { Id = u.UserId, Name = u.UserName })
                .ToList();

            // Modified to include membership type
            var offerDetails = _context.TblOffers
                .Select(o => new {
                    Id = o.OffId,
                    Percentage = o.DiscountPrecentage,
                    MembershipTypeId = o.MemberShipTypesId
                }).ToList();

            var membershipDetails = _context.TblMembershipTypes
                .Select(m => new { Id = m.MemberShipTypesId, Name = m.Name, Price = m.Price })
                .ToList();

            var membershipDurations = _context.TblMembershipTypes
                .ToDictionary(m => m.MemberShipTypesId.ToString(), m => m.MembershipDuration);

            var membershipFeatures = _context.TblMembershipTypes
                .ToDictionary(m => m.MemberShipTypesId.ToString(), m => new {
                    invitationCount = m.invitationCount,
                    totalFreezeDays = m.TotalFreezeDays,
                    freezeCount = m.FreezeCount
                });
            ViewBag.MembershipFeatures = membershipFeatures;

            // Use the filtered activeUsers for the SelectList
            ViewBag.UserId = new SelectList(activeUsers, "UserId", "UserName");
            ViewBag.AllOffers = new SelectList(_context.TblOffers, "OffId", "OfferName");
            ViewBag.MemberShipTypesId = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name");

            ViewBag.MembershipDurations = membershipDurations;
            ViewBag.UserDetails = userDetails;
            ViewBag.OfferDetails = offerDetails;
            ViewBag.MembershipDetails = membershipDetails;
        }
    }
}