using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipType;
using GYMappWeb.Helpers;

namespace GYMappWeb.Controllers
{
    public class TblMembershipTypesController : Controller
    {
        private readonly GYMappWebContext _context;

        public TblMembershipTypesController(GYMappWebContext context)
        {
            _context = context;
        }

        // GET: TblMembershipTypes
        public async Task<IActionResult> Index()
        {
            // Load user Id and UserName from AspNetUsers
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            // Project TblMembershipTypes to the ViewModel
            var membershipTypes = await _context.TblMembershipTypes
                .OrderBy(m => m.Name)
                .Select(m => new TblMemberShipTypeViewModel
                {
                    MemberShipTypesId = m.MemberShipTypesId,
                    Name = m.Name,
                    MembershipDuration = m.MembershipDuration,
                    Price = m.Price,
                    invitationCount = m.invitationCount,
                    Description = m.Description,
                    FreezeCount = m.FreezeCount,
                    TotalFreezeDays = m.TotalFreezeDays,
                    CreatedDate = m.CreatedDate,
                    CreatedBy = m.CreatedBy,
                    CreatedByUserName = m.CreatedBy != null && userNames.ContainsKey(m.CreatedBy)
                        ? userNames[m.CreatedBy]
                        : "Unknown"
                })
                .ToListAsync();

            return View(membershipTypes);
        }


        // GET: TblMembershipTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblMembershipType = await _context.TblMembershipTypes
                .FirstOrDefaultAsync(m => m.MemberShipTypesId == id);
            if (tblMembershipType == null)
            {
                return NotFound();
            }

            return View(tblMembershipType);
        }

        // GET: TblMembershipTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TblMembershipTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,MembershipDuration,Price,invitationCount,Description,FreezeCount,TotalFreezeDays")] TblMembershipType tblMembershipType)
        {
            if (ModelState.IsValid)
            {

                // Set CreatedBy from session
                var userSession = HttpContext.Session.GetUserSession();
                tblMembershipType.CreatedBy = userSession?.Id;

                // Set today's date
                tblMembershipType.CreatedDate = DateTime.Today;

                _context.Add(tblMembershipType);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(tblMembershipType);
        }


        // GET: TblMembershipTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblMembershipType = await _context.TblMembershipTypes.FindAsync(id);
            if (tblMembershipType == null)
            {
                return NotFound();
            }
            var viewModel = ObjectMapper.Mapper.Map<TblMemberShipTypeViewModel>(tblMembershipType);
            return View(viewModel);
        }

        // POST: TblMembershipTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberShipTypesId,Name,MembershipDuration,Price,invitationCount,Description,FreezeCount,TotalFreezeDays")] TblMemberShipTypeViewModel tblMembershipType)
        {
            if (id != tblMembershipType.MemberShipTypesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set CreatedBy and CreatedDate from session or context
                    var userSession = HttpContext.Session.GetUserSession();
                    tblMembershipType.CreatedBy = userSession?.Id;
                    tblMembershipType.CreatedDate = DateTime.Today;

                    var updatedEntity = ObjectMapper.Mapper.Map<TblMembershipType>(tblMembershipType);

                    _context.Update(updatedEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblMembershipTypeExists(tblMembershipType.MemberShipTypesId))
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
            return View(tblMembershipType);
        }

        // GET: TblMembershipTypes/Delete/5
        [HttpGet]
        public async Task<IActionResult> HasRelatedMemberships(int id)
        {
            try
            {
                var hasMemberships = await _context.TblUserMemberShips
                    .AnyAsync(m => m.MemberShipTypesId == id);

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
                var membershipType = await _context.TblMembershipTypes.FindAsync(id);
                if (membershipType == null)
                {
                    return NotFound();
                }

                _context.TblMembershipTypes.Remove(membershipType);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting membership type: {ex.Message}");
            }
        }

        private bool TblMembershipTypeExists(int id)
        {
            return _context.TblMembershipTypes.Any(e => e.MemberShipTypesId == id);
        }
    }
}
