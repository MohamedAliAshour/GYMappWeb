using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.Dashboard;
using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.ViewModels.TblUserMemberShip;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GYMappWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GYMappWebContext _context;

        public HomeController(ILogger<HomeController> logger, GYMappWebContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Convert DateTime.Today to DateOnly for comparison
            var today = DateOnly.FromDateTime(DateTime.Today);
            var nextWeek = today.AddDays(7);

            var dashboardStats = new DashboardViewModel
            {
                TotalMembers = await _context.TblUsers.CountAsync(),
                ActiveMemberships = await _context.TblUserMemberShips
                    .CountAsync(um => um.EndDate >= today),
                ExpiringMemberships = await _context.TblUserMemberShips
                    .CountAsync(um => um.EndDate <= nextWeek && um.EndDate >= today),
                RecentFreezes = await _context.TblMemberShipFreezes
                    .Include(f => f.UserMemberShip)
                    .ThenInclude(um => um.User)
                    .OrderByDescending(f => f.FreezeStartDate)
                    .Take(5)
                    .Select(f => new TblMemberShipFreezeViewModel
                    {
                        MemberShipFreezeId = f.MemberShipFreezeId,
                        UserMemberShipId = f.UserMemberShipId,
                        FreezeStartDate = f.FreezeStartDate,
                        FreezeEndDate = f.FreezeEndDate,
                        Reason = f.Reason,
                        CreatedBy = f.CreatedBy,
                        CreatedDate = f.CreatedDate,
                        UserMemberShip = new TblUserMemberShipViewModel
                        {
                            // Map only the properties you need for display
                            UserName = f.UserMemberShip.User.UserName
                            // Add other UserMemberShip properties as needed
                        }
                    })
                    .ToListAsync()
            };

            return View(dashboardStats);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}