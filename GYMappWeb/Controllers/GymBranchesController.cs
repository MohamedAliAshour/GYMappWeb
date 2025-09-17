using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GYMappWeb.Helpers;
using GYMappWeb.Interface;
using GYMappWeb.ViewModels.GymBranch;
using GYMappWeb.Helper;
using GYMappWeb.Models;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer,User")]
    public class GymBranchesController : Controller
    {
        private readonly IGymBranch _gymBranchService;

        public GymBranchesController(IGymBranch gymBranchService)
        {
            _gymBranchService = gymBranchService;
        }

        // GET: GymBranches
        public async Task<IActionResult> Index(UserParameters userParameters)
        {
            var gymBranches = await _gymBranchService.GetWithPaginations(userParameters);
            return View(gymBranches);
        }

        // GET: GymBranches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymBranch = await _gymBranchService.GetGymBranchDetailsAsync(id.Value);
            if (gymBranch == null)
            {
                return NotFound();
            }

            return View(gymBranch);
        }

        // GET: GymBranches/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GymBranches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GymBranchId,GymName,Location,CreateDate,CreatedBy")] SaveGymBranchViewModel gymBranch)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    await _gymBranchService.Add(gymBranch, userSession?.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(gymBranch);
        }

        // GET: GymBranches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymBranch = _gymBranchService.GetDetailsById(id.Value);
            if (gymBranch == null)
            {
                return NotFound();
            }

            return View(gymBranch);
        }

        // POST: GymBranches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GymBranchId,GymName,Location,CreateDate,CreatedBy")] SaveGymBranchViewModel gymBranchViewModel)
        {
            if (id != gymBranchViewModel.GymBranchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userSession = HttpContext.Session.GetUserSession();
                    await _gymBranchService.Update(gymBranchViewModel, id, userSession?.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(gymBranchViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _gymBranchService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting gym branch: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateGymName(string value, string lang = "en")
        {
            bool exists = await _gymBranchService.CheckNameExist(value);

            string errorMessage = lang == "ar"
                ? "اسم الصالة الرياضية هذا مستخدم بالفعل"
                : "This gym name is already taken";

            return Json(new
            {
                isValid = !exists,
                errorMessage = exists ? errorMessage : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> ValidateLocation(string value, string lang = "en")
        {
            bool exists = await _gymBranchService.CheckLocationExist(value);

            string errorMessage = lang == "ar"
                ? "هذا الموقع مسجل بالفعل"
                : "This location is already registered";

            return Json(new
            {
                isValid = !exists,
                errorMessage = exists ? errorMessage : ""
            });
        }
    }
}