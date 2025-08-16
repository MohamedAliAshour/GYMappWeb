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
    [Authorize(Roles = "Captain,Developer")]
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
            var users = await _userService.GetWithPaginations(userParameters);
            return View(users);
        }

        // GET: TblUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userService.GetUserDetailsAsync(id.Value);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: TblUsers/Create
        public async Task<IActionResult> Create()
        {
            ViewData["UserCode"] = await _userService.GetNextUserCode();
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
                    await _userService.Add(tblUser, userSession?.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
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

            var user = _userService.GetDetailsById(id.Value);
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
        public async Task<IActionResult> ValidateUserName(string value)
        {
            bool exists = await _userService.CheckNameExist(value);
            return Json(new
            {
                isValid = !exists,
                errorMessage = exists ? "This username is already taken" : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> ValidateUserPhone(string value)
        {
            bool exists = await _userService.CheckPhoneExist(value);
            return Json(new
            {
                isValid = !exists,
                errorMessage = exists ? "This phone number is already registered" : ""
            });
        }
    }
}