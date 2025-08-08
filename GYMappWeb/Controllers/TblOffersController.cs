using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.Helpers;
using GYMappWeb.ViewModels.TblUser;

namespace GYMappWeb.Controllers
{
    public class TblOffersController : Controller
    {
        private readonly GYMappWebContext _context;

        public TblOffersController(GYMappWebContext context)
        {
            _context = context;
        }

        // GET: TblOffers
        public async Task<IActionResult> Index()
        {
            // Load user Id and UserName from AspNetUsers
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            // Project TblOffers to the ViewModel
            var offers = await _context.TblOffers
                .Include(o => o.MemberShipTypes)
                .OrderBy(o => o.OfferName)
                .Select(o => new TblOfferViewModel
                {
                    OffId = o.OffId,
                    OfferName = o.OfferName,
                    DiscountPrecentage = o.DiscountPrecentage,
                    MemberShipTypesId = o.MemberShipTypesId,
                    CreatedBy = o.CreatedBy,
                    CreatedDate = o.CreatedDate,
                    CreatedByUserName = o.CreatedBy != null && userNames.ContainsKey(o.CreatedBy)
                        ? userNames[o.CreatedBy]
                        : "Unknown",
                    MemberShipTypes = o.MemberShipTypes
                })
                .ToListAsync();

            return View(offers);
        }


        // GET: TblOffers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblOffer = await _context.TblOffers
                .Include(t => t.MemberShipTypes)
                .FirstOrDefaultAsync(m => m.OffId == id);
            if (tblOffer == null)
            {
                return NotFound();
            }

            return View(tblOffer);
        }

        // GET: TblOffers/Create
        public IActionResult Create()
        {
            ViewData["MemberShipTypesId"] = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name");
            return View();
        }

        // POST: TblOffers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OffId,OfferName,DiscountPrecentage,MemberShipTypesId")] TblOfferViewModel tblOffer)
        {
            if (ModelState.IsValid)
            {
                // Get user session info
                var userSession = HttpContext.Session.GetUserSession();
                tblOffer.CreatedBy = userSession?.Id;

                tblOffer.CreatedDate = DateTime.Today;

                var entity = ObjectMapper.Mapper.Map<TblOffer>(tblOffer);

                _context.Add(entity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MemberShipTypesId"] = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name", tblOffer.MemberShipTypesId);
            return View(tblOffer);
        }


        // GET: TblOffers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblOffer = await _context.TblOffers.FindAsync(id);
            if (tblOffer == null)
            {
                return NotFound();
            }
            var viewModel = ObjectMapper.Mapper.Map<TblOfferViewModel>(tblOffer);

            ViewData["MemberShipTypesId"] = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name", tblOffer.MemberShipTypesId);
            return View(viewModel);
        }

        // POST: TblOffers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OffId,OfferName,DiscountPrecentage,MemberShipTypesId")] TblOfferViewModel tblOffer)
        {
            if (id != tblOffer.OffId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Set CreatedBy to session Id and CreatedDate to today
                    var userSession = HttpContext.Session.GetUserSession();
                    tblOffer.CreatedBy = userSession?.Id;
                    tblOffer.CreatedDate = DateTime.Today;

                    var updatedEntity = ObjectMapper.Mapper.Map<TblOffer>(tblOffer);

                    _context.Update(updatedEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblOfferExists(tblOffer.OffId))
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
            ViewData["MemberShipTypesId"] = new SelectList(_context.TblMembershipTypes, "MemberShipTypesId", "Name", tblOffer.MemberShipTypesId);
            return View(tblOffer);
        }

        // GET: TblOffers/Delete/5
        [HttpGet]
        public async Task<IActionResult> HasRelatedMemberships(int id)
        {
            try
            {
                var hasMemberships = await _context.TblUserMemberShips
                    .AnyAsync(m => m.OffId == id);

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
                var offer = await _context.TblOffers.FindAsync(id);
                if (offer == null)
                {
                    return NotFound();
                }

                _context.TblOffers.Remove(offer);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting offer: {ex.Message}");
            }
        }

        private bool TblOfferExists(int id)
        {
            return _context.TblOffers.Any(e => e.OffId == id);
        }
    }
}
