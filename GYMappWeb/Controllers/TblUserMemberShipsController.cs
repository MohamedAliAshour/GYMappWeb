using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblUserMemberShip;
using GYMappWeb.ViewModels.TblMemberShipType;
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.Helpers;

namespace GYMappWeb.Controllers
{
    public class TblUserMemberShipsController : Controller
    {
        private readonly GYMappWebContext _context;

        public TblUserMemberShipsController(GYMappWebContext context)
        {
            _context = context;
        }

        // GET: TblUserMemberShips
        public async Task<IActionResult> Index()
        {
            await UpdateExpiredMemberships();

            // Get the Identity users dictionary for CreatedBy lookup
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            // Project into the view model with ordering by UserCode
            var userMemberships = await _context.TblUserMemberShips
                .Include(t => t.MemberShipTypes)
                .Include(t => t.Off)
                .Include(t => t.User)
                .OrderBy(m => m.User.UserCode) // Order by UserCode first
                .ThenBy(m => m.User.UserName)  // Then by UserName
                .Select(m => new TblUserMemberShipViewModel
                {
                    UserMemberShipId = m.UserMemberShipId,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    IsActive = m.IsActive,
                    invitationUsed = m.invitationUsed,
                    TotalFreezedDays = m.TotalFreezedDays,
                    OffId = m.OffId,
                    UserId = m.UserId,
                    MemberShipTypesId = m.MemberShipTypesId,
                    CreatedBy = m.CreatedBy,
                    CreatedDate = m.CreatedDate,
                    CreatedByUserName = m.CreatedBy != null && userNames.ContainsKey(m.CreatedBy)
                        ? userNames[m.CreatedBy]
                        : "Unknown",
                    MemberShipTypes = new TblMemberShipTypeViewModel
                    {
                        MemberShipTypesId = m.MemberShipTypes.MemberShipTypesId,
                        Name = m.MemberShipTypes.Name
                    },
                    Off = m.Off == null ? null : new TblOfferViewModel
                    {
                        OffId = m.Off.OffId,
                        OfferName = m.Off.OfferName,
                        DiscountPrecentage = m.Off.DiscountPrecentage
                    },
                    User = new TblUserViewModel
                    {
                        UserId = m.User.UserId,
                        UserName = m.User.UserName,
                        UserPhone = m.User.UserPhone,
                        UserCode = m.User.UserCode
                    }
                })
                .ToListAsync();

            return View(userMemberships);
        }


        private async Task UpdateExpiredMemberships()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Get active memberships that have expired
            var expiredMemberships = await _context.TblUserMemberShips
                .Include(m => m.User)
                .Where(m => m.EndDate <= today && m.IsActive)
                .ToListAsync();

            if (!expiredMemberships.Any())
                return;

            foreach (var membership in expiredMemberships)
            {
                membership.IsActive = false;

                // Check if this is the user's only active membership
                var hasOtherActiveMemberships = await _context.TblUserMemberShips
                    .AnyAsync(m => m.UserId == membership.UserId &&
                                  m.UserMemberShipId != membership.UserMemberShipId &&
                                  m.IsActive);

                // Deactivate user if no other active memberships exist
                if (membership.User != null && !hasOtherActiveMemberships)
                {
                    membership.User.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
        }



        // GET: TblUserMemberShips/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUserMemberShip = await _context.TblUserMemberShips
                .Include(t => t.MemberShipTypes)
                .Include(t => t.Off)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.UserMemberShipId == id);
            if (tblUserMemberShip == null)
            {
                return NotFound();
            }

            return View(tblUserMemberShip);
        }

        // GET: TblUserMemberShips/Create
        public IActionResult Create()
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

            return View();
        }


        // POST: TblUserMemberShips/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserMemberShipId,StartDate,EndDate,IsActive,invitationUsed,TotalFreezedDays,OffId,UserId,MemberShipTypesId")] TblUserMemberShipViewModel tblUserMemberShip)
        {
            if (ModelState.IsValid)
            {
                // Check if user already has an active membership
                var activeMembership = await _context.TblUserMemberShips
                    .Where(m => m.UserId == tblUserMemberShip.UserId && m.IsActive)
                    .OrderByDescending(m => m.EndDate)
                    .FirstOrDefaultAsync();

                if (activeMembership != null)
                {
                    ModelState.AddModelError(string.Empty, $"This user already has an active membership that ends on {activeMembership.EndDate:yyyy-MM-dd}");
                    ViewData["MemberShipTypesId"] = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name", tblUserMemberShip.MemberShipTypesId);
                    ViewData["OffId"] = new SelectList(_context.TblOffers, "OffId", "OfferName", tblUserMemberShip.OffId);
                    ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserName", tblUserMemberShip.UserId);
                    return View(tblUserMemberShip);
                }

                // Set CreatedBy from session and CreatedDate to today's date
                var userSession = HttpContext.Session.GetUserSession();
                tblUserMemberShip.CreatedBy = userSession?.Id;
                tblUserMemberShip.CreatedDate = DateTime.Today;
                tblUserMemberShip.IsActive = true; // Set IsActive to true by default

                // Update the corresponding user's IsActive status to true
                var user = await _context.TblUsers.FindAsync(tblUserMemberShip.UserId);
                if (user != null)
                {
                    user.IsActive = true;
                    _context.Update(user);
                }

                // Map ViewModel to Entity using AutoMapper
                var entity = ObjectMapper.Mapper.Map<TblUserMemberShip>(tblUserMemberShip);

                // Ensure these values are set in case they're not mapped automatically
                entity.CreatedBy = userSession?.Id;
                entity.CreatedDate = DateTime.Today;

                _context.Add(entity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["MemberShipTypesId"] = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name", tblUserMemberShip.MemberShipTypesId);
            ViewData["OffId"] = new SelectList(_context.TblOffers, "OffId", "OfferName", tblUserMemberShip.OffId);
            ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserName", tblUserMemberShip.UserId);
            return View(tblUserMemberShip);
        }


        // GET: TblUserMemberShips/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUserMemberShip = await _context.TblUserMemberShips.FindAsync(id);
            if (tblUserMemberShip == null)
            {
                return NotFound();
            }
            ViewData["MemberShipTypesId"] = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name", tblUserMemberShip.MemberShipTypesId);
            ViewData["OffId"] = new SelectList(_context.TblOffers, "OffId", "OfferName", tblUserMemberShip.OffId);
            ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserName", tblUserMemberShip.UserId);
            return View(tblUserMemberShip);
        }

        // POST: TblUserMemberShips/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserMemberShipId,StartDate,EndDate,IsActive,invitationUsed,TotalFreezedDays,OffId,UserId,MemberShipTypesId,CreatedBy,CreatedDate")] TblUserMemberShip tblUserMemberShip)
        {
            if (id != tblUserMemberShip.UserMemberShipId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblUserMemberShip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblUserMemberShipExists(tblUserMemberShip.UserMemberShipId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MemberShipTypesId"] = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name", tblUserMemberShip.MemberShipTypesId);
            ViewData["OffId"] = new SelectList(_context.TblOffers, "OffId", "OfferName", tblUserMemberShip.OffId);
            ViewData["UserId"] = new SelectList(_context.TblUsers, "UserId", "UserName", tblUserMemberShip.UserId);
            return View(tblUserMemberShip);
        }

        // GET: TblUserMemberShips/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFreezes(int id)
        {
            try
            {
                var freezes = await _context.TblMemberShipFreezes
                    .Where(f => f.UserMemberShipId == id)
                    .ToListAsync();

                _context.TblMemberShipFreezes.RemoveRange(freezes);
                await _context.SaveChangesAsync();

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
                var membership = await _context.TblUserMemberShips.FindAsync(id);
                if (membership == null)
                {
                    return NotFound();
                }

                _context.TblUserMemberShips.Remove(membership);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting membership: {ex.Message}");
            }
        }

        private bool TblUserMemberShipExists(int id)
        {
            return _context.TblUserMemberShips.Any(e => e.UserMemberShipId == id);
        }


        [HttpGet]
        public async Task<IActionResult> CheckActiveMembership(int userId)
        {
            var activeMembership = await _context.TblUserMemberShips
                .Where(m => m.UserId == userId && m.IsActive)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefaultAsync();

            return Json(new
            {
                hasActiveMembership = activeMembership != null,
                endDate = activeMembership?.EndDate.ToString("yyyy-MM-dd") // Only return end date
            });
        }
    }
}
