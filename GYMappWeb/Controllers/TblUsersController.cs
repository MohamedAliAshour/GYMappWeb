using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;
using Microsoft.AspNetCore.Authorization;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.Helpers;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer")]
    public class TblUsersController : Controller
    {
        private readonly GYMappWebContext _context;

        public TblUsersController(GYMappWebContext context)
        {
            _context = context;
        }

        // GET: TblUsers
        public async Task<IActionResult> Index()
        {
            var userNames = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            var users = await _context.TblUsers
                .OrderBy(u => u.UserCode) // Order by UserCode
                .ThenBy(u => u.UserName)  // Then by UserName
                .Select(u => new TblUserViewModel
                {
                    UserId = u.UserId,
                    UserCode = u.UserCode,
                    UserName = u.UserName,
                    UserPhone = u.UserPhone,
                    IsActive = u.IsActive,
                    Notes = u.Notes,
                    CreatedDate = u.CreatedDate,
                    CreatedBy = u.CreatedBy,
                    CreatedByUserName = u.CreatedBy != null && userNames.ContainsKey(u.CreatedBy)
                        ? userNames[u.CreatedBy]
                        : "Unknown"
                })
                .ToListAsync();

            return View(users);
        }


        // GET: TblUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUser = await _context.TblUsers
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (tblUser == null)
            {
                return NotFound();
            }

            return View(tblUser);
        }

        // GET: TblUsers/Create
        public IActionResult Create()
        {

            var lastUser = _context.TblUsers
               .OrderByDescending(u => u.UserCode)
               .FirstOrDefault();

            ViewData["UserCode"] = lastUser != null ? lastUser.UserCode + 1 : 1;

            return View();
        }

        // POST: TblUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserCode,UserName,UserPhone,IsActive,RolesId,Notes")] TblUserViewModel tblUser)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate username
                bool usernameExists = await _context.TblUsers
                    .AnyAsync(u => u.UserName.ToLower() == tblUser.UserName.ToLower());

                if (usernameExists)
                {
                    ModelState.AddModelError("UserName", "A user with this name already exists.");
                    ViewData["UserCode"] = tblUser.UserCode;
                    return View(tblUser);
                }

                // Check for duplicate phone number
                bool phoneExists = await _context.TblUsers
                    .AnyAsync(u => u.UserPhone == tblUser.UserPhone);

                if (phoneExists)
                {
                    ModelState.AddModelError("UserPhone", "This phone number is already registered.");
                    ViewData["UserCode"] = tblUser.UserCode;
                    return View(tblUser);
                }

                // Ensure UserCode is set correctly
                var lastUser = await _context.TblUsers
                    .OrderByDescending(u => u.UserCode)
                    .FirstOrDefaultAsync();

                tblUser.UserCode = lastUser != null ? lastUser.UserCode + 1 : 1;

                // Set CreatedBy to session Id and CreatedDate to today
                var userSession = HttpContext.Session.GetUserSession();
                tblUser.CreatedBy = userSession?.Id;
                tblUser.CreatedDate = DateTime.Today;

                var entity = ObjectMapper.Mapper.Map<TblUser>(tblUser);

                _context.Add(entity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserCode"] = tblUser.UserCode;
            return View(tblUser);
        }

        // GET: TblUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblUser = await _context.TblUsers.FindAsync(id);
            if (tblUser == null)
            {
                return NotFound();
            }
            var viewModel = ObjectMapper.Mapper.Map<TblUserViewModel>(tblUser);

            return View(viewModel);
        }

        // POST: TblUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserCode,UserName,UserPhone,IsActive,Notes")] TblUserViewModel tblUserViewModel)
        {
            if (id != tblUserViewModel.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Set CreatedBy to session Id and CreatedDate to today
                    var userSession = HttpContext.Session.GetUserSession();
                    tblUserViewModel.CreatedBy = userSession?.Id;
                    tblUserViewModel.CreatedDate = DateTime.Today;

                    var updatedEntity = ObjectMapper.Mapper.Map<TblUser>(tblUserViewModel);

                    _context.Update(updatedEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TblUserExists(tblUserViewModel.UserId))
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
            return View(tblUserViewModel);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRelatedRecords(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // Delete all freezes related to this user's memberships
                var memberships = await _context.TblUserMemberShips
                    .Where(m => m.UserId == id)
                    .ToListAsync();

                foreach (var membership in memberships)
                {
                    var freezes = await _context.TblMemberShipFreezes
                        .Where(f => f.UserMemberShipId == membership.UserMemberShipId)
                        .ToListAsync();

                    _context.TblMemberShipFreezes.RemoveRange(freezes);
                }

                // Delete all memberships for this user
                _context.TblUserMemberShips.RemoveRange(memberships);

                // Delete any other related records here
                // Example: _context.OtherRelatedEntities.RemoveRange(...);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error deleting related records: {ex.Message}");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _context.TblUsers.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                _context.TblUsers.Remove(user);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting user: {ex.Message}");
            }
        }

        private bool TblUserExists(int id)
        {
            return _context.TblUsers.Any(e => e.UserId == id);
        }




        [HttpGet]
        public async Task<IActionResult> ValidateUserName(string value)
        {
            bool exists = await _context.TblUsers.AnyAsync(u => u.UserName.ToLower() == value.ToLower());
            return Json(new
            {
                isValid = !exists,
                errorMessage = exists ? "This username is already taken" : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> ValidateUserPhone(string value)
        {
            bool exists = await _context.TblUsers.AnyAsync(u => u.UserPhone == value);
            return Json(new
            {
                isValid = !exists,
                errorMessage = exists ? "This phone number is already registered" : ""
            });
        }


    }
}
