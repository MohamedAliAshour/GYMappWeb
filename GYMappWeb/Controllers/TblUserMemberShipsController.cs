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
using Microsoft.AspNetCore.Authorization;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer,User")]
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
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;
            
            var memberships = await _membershipService.GetAllUserMembershipsAsync(userParameters, gymBranchId);
            return View(memberships);
        }

        public IActionResult Create()
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;
            
            SetupCreateViewBag(gymBranchId);
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
                    var gymBranchId = userSession.GymBranchId ?? 1;
                    
                    await _membershipService.AddMembershipAsync(model, userSession?.Id, gymBranchId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var userSessionForView = HttpContext.Session.GetUserSession();
            var gymBranchIdForView = userSessionForView.GymBranchId ?? 1;
            
            SetupCreateViewBag(gymBranchIdForView);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFreezes(int id)
        {
            try
            {
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;
                
                await _membershipService.DeleteFreezesAsync(id, gymBranchId);
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
                var userSession = HttpContext.Session.GetUserSession();
                var gymBranchId = userSession.GymBranchId ?? 1;
                
                await _membershipService.DeleteMembershipAsync(id, gymBranchId);
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
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;
            
            var hasActive = await _membershipService.HasActiveMembershipAsync(userId, gymBranchId);
            return Json(new { hasActiveMembership = hasActive });
        }

        private void SetupCreateViewBag(int gymBranchId)
        {
            // Filter active users for current gym branch
            var activeUsers = _context.TblUsers
                .Where(u => u.IsActive == false && u.GymBranchId == gymBranchId) // Added gym branch filter
                .ToList();

            var userDetails = activeUsers
                .Select(u => new { Id = u.UserId, Name = u.UserName, Code = u.UserCode })
                .ToList();

            // Filter offers for current gym branch
            var offerDetails = _context.TblOffers
                .Where(o => o.IsActive == true && o.GymBranchId == gymBranchId) // Added gym branch filter
                .Select(o => new
                {
                    Id = o.OffId,
                    Percentage = o.DiscountPrecentage,
                    MembershipTypeId = o.MemberShipTypesId
                }).ToList();

            // Filter membership types for current gym branch
            var membershipDetails = _context.TblMembershipTypes
                .Where(m => m.IsActive == true && m.GymBranchId == gymBranchId) // Added gym branch filter
                .Select(m => new { Id = m.MemberShipTypesId, Name = m.Name, Price = m.Price })
                .ToList();

            var membershipDurations = _context.TblMembershipTypes
                .Where(m => m.IsActive == true && m.GymBranchId == gymBranchId) // Added gym branch filter
                .ToDictionary(m => m.MemberShipTypesId.ToString(), m => m.MembershipDuration);

            var membershipFeatures = _context.TblMembershipTypes
                .Where(m => m.IsActive == true && m.GymBranchId == gymBranchId) // Added gym branch filter
                .ToDictionary(m => m.MemberShipTypesId.ToString(), m => new {
                    invitationCount = m.invitationCount,
                    totalFreezeDays = m.TotalFreezeDays,
                    freezeCount = m.FreezeCount
                });
            
            ViewBag.MembershipFeatures = membershipFeatures;
            ViewBag.UserId = new SelectList(activeUsers, "UserId", "UserName");
            ViewBag.AllOffers = new SelectList(_context.TblOffers.Where(o => o.IsActive == true && o.GymBranchId == gymBranchId), "OffId", "OfferName");
            ViewBag.MemberShipTypesId = new SelectList(_context.TblMembershipTypes.Where(m => m.IsActive == true && m.GymBranchId == gymBranchId), "MemberShipTypesId", "Name");
            ViewBag.MembershipDurations = membershipDurations;
            ViewBag.UserDetails = userDetails;
            ViewBag.OfferDetails = offerDetails;
            ViewBag.MembershipDetails = membershipDetails;
        }

        [HttpGet]
        public IActionResult SearchUsers(string term, int gymBranchId)
        {
            var users = _context.TblUsers
                .Where(u => u.IsActive == false &&
                           u.GymBranchId == gymBranchId &&
                           (u.UserName.Contains(term) || u.UserCode.ToString().Contains(term)))
                .Select(u => new
                {
                    id = u.UserId,
                    name = u.UserName,
                    code = u.UserCode
                })
                .Take(10)
                .ToList();

            return Json(users);
        }
    }
}