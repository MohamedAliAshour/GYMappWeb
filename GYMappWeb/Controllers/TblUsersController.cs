using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.Models;
using GYMappWeb.Helper;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer,User")]
    public class TblUsersController : Controller
    {
        private readonly ITblUser _userService;

        public TblUsersController(ITblUser userService)
        {
            _userService = userService;
        }

        // GET: TblUsers
        public async Task<IActionResult> Index(UserParameters userParameters)
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            // Temporary debug
            Console.WriteLine($"Current user gym branch ID: {gymBranchId}");

            var users = await _userService.GetWithPaginations(userParameters, gymBranchId);

            // Check how many users were returned
            Console.WriteLine($"Users found: {users.Items.Count}");

            return View(users);
        }

        // GET: TblUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var user = await _userService.GetUserDetailsAsync(id.Value, gymBranchId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: TblUsers/Create
        public async Task<IActionResult> Create()
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            ViewData["UserCode"] = await _userService.GetNextUserCode(gymBranchId);
            return View();
        }

        // POST: TblUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserCode,UserName,UserPhone,IsActive,RolesId,Notes")] SaveTblUserViewModel tblUser)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    var gymBranchId = userSession.GymBranchId ?? 1;

                    await _userService.Add(tblUser, userSession?.Id, gymBranchId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            var userSessionForView = HttpContext.Session.GetUserSession();
            var gymBranchIdForView = userSessionForView.GymBranchId ?? 1;

            ViewData["UserCode"] = await _userService.GetNextUserCode(gymBranchIdForView);
            return View(tblUser);
        }

        // GET: TblUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            var user = _userService.GetDetailsById(id.Value, gymBranchId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: TblUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserCode,UserName,UserPhone,IsActive,Notes")] SaveTblUserViewModel tblUserViewModel)
        {
            if (id != tblUserViewModel.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    await _userService.Update(tblUserViewModel, id, userSession?.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(tblUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRelatedRecords(int id)
        {
            try
            {
                await _userService.DeleteRelatedRecords(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting related records: {ex.Message}");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _userService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting user: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateUserPhone(string value, string lang = "en")
        {
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            bool exists = await _userService.CheckPhoneExist(value, gymBranchId);

            string errorMessage = lang == "ar"
                ? "رقم الهاتف هذا مسجل بالفعل"
                : "This phone number is already registered";

            return Json(new
            {
                isValid = !exists,
                errorMessage = exists ? errorMessage : ""
            });
        }

    }
}