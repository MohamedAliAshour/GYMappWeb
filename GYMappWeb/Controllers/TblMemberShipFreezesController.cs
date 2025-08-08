using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.Helpers;
using Microsoft.AspNetCore.Authorization;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.ViewModels.TblUserMemberShip;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer")]
    public class TblMemberShipFreezesController : Controller
    {
        private readonly GYMappWebContext _context;

        public TblMemberShipFreezesController(GYMappWebContext context)
        {
            _context = context;
        }

        // GET: TblMemberShipFreezes
        public async Task<IActionResult> Index()
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var freezes = await _context.TblMemberShipFreezes
                .Include(f => f.UserMemberShip)
                .ThenInclude(um => um.User)
                .OrderBy(f => f.FreezeStartDate)
                .Select(f => new TblMemberShipFreezeViewModel
                {
                    MemberShipFreezeId = f.MemberShipFreezeId,
                    UserMemberShipId = f.UserMemberShipId,
                    FreezeStartDate = f.FreezeStartDate,
                    FreezeEndDate = f.FreezeEndDate,
                    Reason = f.Reason,
                    CreatedDate = f.CreatedDate,
                    CreatedBy = f.CreatedBy,
                    CreatedByUserName = f.CreatedBy != null && userNames.ContainsKey(f.CreatedBy)
                        ? userNames[f.CreatedBy]
                        : "Unknown",
                    UserMemberShip = new TblUserMemberShipViewModel
                    {
                        UserMemberShipId = f.UserMemberShip.UserMemberShipId,
                        User = new TblUserViewModel
                        {
                            UserName = f.UserMemberShip.User.UserName
                        }
                    }
                })
                .ToListAsync();

            return View(freezes);
        }

        // GET: TblMemberShipFreezes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var freeze = await _context.TblMemberShipFreezes
                .Include(f => f.UserMemberShip)
                .FirstOrDefaultAsync(m => m.MemberShipFreezeId == id);

            if (freeze == null)
            {
                return NotFound();
            }

            var viewModel = ObjectMapper.Mapper.Map<TblMemberShipFreezeViewModel>(freeze);
            return View(viewModel);
        }


        [HttpGet]
        public IActionResult GetFreezeRecords(int userMembershipId)
        {
            var freezeRecords = _context.TblMemberShipFreezes
                .Where(f => f.UserMemberShipId == userMembershipId)
                .OrderByDescending(f => f.FreezeStartDate)
                .Select(f => new {
                    freezeStartDate = f.FreezeStartDate.ToString("yyyy-MM-dd"),
                    freezeEndDate = f.FreezeEndDate.ToString("yyyy-MM-dd"),
                    reason = f.Reason
                })
                .ToList();

            return Json(freezeRecords);
        }


        [HttpGet]
        public async Task<IActionResult> GetMembershipFreezeDetails(int userMembershipId)
        {
            var membership = await _context.TblUserMemberShips
                .Include(um => um.MemberShipTypes)
                .FirstOrDefaultAsync(um => um.UserMemberShipId == userMembershipId);

            if (membership == null)
            {
                return NotFound();
            }

            var freezes = await _context.TblMemberShipFreezes
                .Where(f => f.UserMemberShipId == userMembershipId)
                .ToListAsync();

            // Calculate used freeze days
            int usedFreezeDays = 0;
            foreach (var freeze in freezes)
            {
                var startDate = new DateTime(freeze.FreezeStartDate.Year, freeze.FreezeStartDate.Month, freeze.FreezeStartDate.Day);
                var endDate = new DateTime(freeze.FreezeEndDate.Year, freeze.FreezeEndDate.Month, freeze.FreezeEndDate.Day);
                usedFreezeDays += (endDate - startDate).Days + 1; // +1 to include both days
            }

            var totalAllowedFreezeDays = membership.MemberShipTypes?.TotalFreezeDays ?? 0;
            var remainingFreezeDays = Math.Max(0, totalAllowedFreezeDays - usedFreezeDays);

            var totalFreezeCount = membership.MemberShipTypes?.FreezeCount ?? 0;
            var remainingFreezeCount = Math.Max(0, totalFreezeCount - freezes.Count);

            return Json(new
            {
                freezeCount = freezes.Count,
                remainingFreezeCount = remainingFreezeCount,
                totalFreezeDays = totalAllowedFreezeDays,
                remainingFreezeDays = remainingFreezeDays
            });
        }

        // Modified Create action to include membership type in the select list
        public IActionResult Create()
        {
            ViewData["UserMemberShipId"] = new SelectList(
                _context.TblUserMemberShips
                    .Include(um => um.User)
                    .Include(um => um.MemberShipTypes)
                    .Where(um => um.IsActive == true) // Only active memberships
                    .Select(um => new {
                        um.UserMemberShipId,
                        UserName = $"{um.User.UserName} ({um.MemberShipTypes.Name})"
                    }),
                "UserMemberShipId",
                "UserName");
            return View();
        }

        // POST: TblMemberShipFreezes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserMemberShipId,FreezeStartDate,FreezeEndDate,Reason")] TblMemberShipFreezeViewModel freezeViewModel)
        {
            if (ModelState.IsValid)
            {
                // Convert DateOnly to DateTime for calculation
                var startDate = new DateTime(freezeViewModel.FreezeStartDate.Year, freezeViewModel.FreezeStartDate.Month, freezeViewModel.FreezeStartDate.Day);
                var endDate = new DateTime(freezeViewModel.FreezeEndDate.Year, freezeViewModel.FreezeEndDate.Month, freezeViewModel.FreezeEndDate.Day);
                var freezeDuration = (endDate - startDate).Days + 1; // Add 1 to include both days

                // Get the associated membership
                var membership = await _context.TblUserMemberShips
                    .FirstOrDefaultAsync(m => m.UserMemberShipId == freezeViewModel.UserMemberShipId);

                if (membership != null)
                {
                    // Convert membership end date if it's DateOnly
                    var membershipEndDate = new DateTime(membership.EndDate.Year, membership.EndDate.Month, membership.EndDate.Day);

                    // Update the membership's total freeze days
                    membership.TotalFreezedDays = (membership.TotalFreezedDays ?? 0) + freezeDuration;

                    // Extend the membership end date by the freeze duration
                    membership.EndDate = DateOnly.FromDateTime(membershipEndDate.AddDays(freezeDuration));

                    _context.Update(membership);
                }

                // Create the freeze record
                var userSession = HttpContext.Session.GetUserSession();
                freezeViewModel.CreatedBy = userSession?.Id;
                freezeViewModel.CreatedDate = DateTime.Today;

                var entity = ObjectMapper.Mapper.Map<TblMemberShipFreeze>(freezeViewModel);
                _context.Add(entity);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate ViewBag if validation fails
            ViewBag.UserMemberShipId = new SelectList(
                _context.TblUserMemberShips
                    .Include(um => um.User)
                    .Include(um => um.MemberShipTypes)
                    .Where(um => um.IsActive == true)
                    .Select(um => new {
                        um.UserMemberShipId,
                        UserName = $"{um.User.UserName} ({um.MemberShipTypes.Name})"
                    }),
                "UserMemberShipId",
                "UserName");

            return View(freezeViewModel);
        }

        // GET: TblMemberShipFreezes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var freeze = await _context.TblMemberShipFreezes.FindAsync(id);
            if (freeze == null)
            {
                return NotFound();
            }

            var viewModel = ObjectMapper.Mapper.Map<TblMemberShipFreezeViewModel>(freeze);
            ViewData["UserMemberShipId"] = new SelectList(
                _context.TblUserMemberShips
                    .Include(um => um.User)
                    .Select(um => new {
                        um.UserMemberShipId,
                        UserName = um.User.UserName
                    }),
                "UserMemberShipId",
                "UserName",
                freeze.UserMemberShipId);

            return View(viewModel);
        }

        // POST: TblMemberShipFreezes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberShipFreezeId,UserMemberShipId,FreezeStartDate,FreezeEndDate,Reason")] TblMemberShipFreezeViewModel freezeViewModel)
        {
            if (id != freezeViewModel.MemberShipFreezeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    freezeViewModel.CreatedBy = userSession?.Id;
                    freezeViewModel.CreatedDate = DateTime.Today;

                    var entity = ObjectMapper.Mapper.Map<TblMemberShipFreeze>(freezeViewModel);

                    _context.Update(entity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblMemberShipFreezeExists(freezeViewModel.MemberShipFreezeId))
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

            ViewData["UserMemberShipId"] = new SelectList(
                _context.TblUserMemberShips
                    .Include(um => um.User)
                    .Select(um => new {
                        um.UserMemberShipId,
                        UserName = um.User.UserName
                    }),
                "UserMemberShipId",
                "UserName",
                freezeViewModel.UserMemberShipId);

            return View(freezeViewModel);
        }

        // GET: TblMemberShipFreezes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var freeze = await _context.TblMemberShipFreezes
                .Include(f => f.UserMemberShip)
                .FirstOrDefaultAsync(m => m.MemberShipFreezeId == id);

            if (freeze == null)
            {
                return NotFound();
            }

            var viewModel = ObjectMapper.Mapper.Map<TblMemberShipFreezeViewModel>(freeze);
            return View(viewModel);
        }

        // POST: TblMemberShipFreezes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var freeze = await _context.TblMemberShipFreezes.FindAsync(id);
                if (freeze == null)
                {
                    return NotFound();
                }

                _context.TblMemberShipFreezes.Remove(freeze);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting freeze: {ex.Message}");
            }
        }
        private bool TblMemberShipFreezeExists(int id)
        {
            return _context.TblMemberShipFreezes.Any(e => e.MemberShipFreezeId == id);
        }
    }
}