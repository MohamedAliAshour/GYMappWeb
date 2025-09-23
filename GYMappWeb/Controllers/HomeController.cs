using GYMappWeb.Areas.Identity.Data;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.Dashboard;
using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.ViewModels.TblUserMemberShip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using GYMappWeb.Helper;
using System.Globalization;

namespace GYMappWeb.Controllers
{
    [Authorize(Roles = "Captain,Developer,User")]
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
            var userSession = HttpContext.Session.GetUserSession();
            var gymBranchId = userSession.GymBranchId ?? 1;

            // Convert DateTime.Today to DateOnly for comparison
            var today = DateOnly.FromDateTime(DateTime.Today);
            var nextWeek = today.AddDays(7);

            // Get today's check-ins
            var todaysCheckins = await _context.Checkins
                .Where(c => c.GymBranchId == gymBranchId &&
                           c.CheckinDate.Date == DateTime.Today)
                .CountAsync();

            // Get active memberships count
            var activeMemberships = await _context.TblUserMemberShips
                .Include(um => um.User)
                .Where(um => um.EndDate >= today &&
                            um.IsActive &&
                            um.User.GymBranchId == gymBranchId)
                .CountAsync();

            // Get total members
            var totalMembers = await _context.TblUsers
                .Where(u => u.GymBranchId == gymBranchId)
                .CountAsync();

            // Get expiring memberships count
            var expiringMemberships = await _context.TblUserMemberShips
                .Include(um => um.User)
                .Where(um => um.EndDate <= nextWeek &&
                            um.EndDate >= today &&
                            um.IsActive &&
                            um.User.GymBranchId == gymBranchId)
                .CountAsync();

            // Get new members this month
            var newMembersThisMonth = await _context.TblUsers
                .Where(u => u.GymBranchId == gymBranchId &&
                           u.CreatedDate.Month == DateTime.Now.Month &&
                           u.CreatedDate.Year == DateTime.Now.Year)
                .CountAsync();

            // Get active offers for this gym branch
            var activeOffers = await _context.TblOffers
                .Where(o => o.GymBranchId == gymBranchId && o.IsActive)
                .CountAsync();

            // Calculate percentages based on your requirements
            var maxCheckins = 200; // Your gym's check-in capacity

            var checkinPercentage = todaysCheckins > 0 ?
                (todaysCheckins / (double)maxCheckins) * 100 : 0;
            checkinPercentage = Math.Min(checkinPercentage, 100);

            // Active Memberships: (Active Memberships / Total Members) percentage
            var activeMembershipPercentage = totalMembers > 0 ?
                (activeMemberships / (double)totalMembers) * 100 : 0;
            activeMembershipPercentage = Math.Min(activeMembershipPercentage, 100);

            // Expiring This Week: (Expiring / Active Memberships) percentage
            var expiringPercentage = activeMemberships > 0 ?
                (expiringMemberships / (double)activeMemberships) * 100 : 0;
            expiringPercentage = Math.Min(expiringPercentage, 100);

            // New Members: (New Members / 100) percentage
            var newMembersPercentage = newMembersThisMonth > 0 ?
                (newMembersThisMonth / 100.0) * 100 : 0;
            newMembersPercentage = Math.Min(newMembersPercentage, 100);

            // Get membership trends data for the chart
            var membershipTrends = await _context.TblUserMemberShips
                .Include(um => um.User)
                .Where(um => um.User.GymBranchId == gymBranchId &&
                            um.StartDate.Year == DateTime.Now.Year)
                .GroupBy(um => um.StartDate.Month)
                .Select(g => new {
                    Month = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            // Prepare data for the chart
            var months = new List<string>();
            var membershipData = new List<int>();

            for (int i = 1; i <= 12; i++)
            {
                var monthData = membershipTrends.FirstOrDefault(m => m.Month == i);
                months.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i));
                membershipData.Add(monthData?.Count ?? 0);
            }

            var dashboardStats = new DashboardViewModel
            {
                TotalMembers = totalMembers,
                ActiveMemberships = activeMemberships,
                TodaysCheckins = todaysCheckins,
                ExpiringMemberships = expiringMemberships,
                NewMembersThisMonth = newMembersThisMonth,

                // Add the percentage calculations
                MaxCheckins = maxCheckins,
                CheckinPercentage = checkinPercentage,
                ActiveMembershipPercentage = activeMembershipPercentage,
                ExpiringPercentage = expiringPercentage,
                NewMembersPercentage = newMembersPercentage,

                ActiveOffers = activeOffers,

                // Membership trends data
                MembershipTrendMonths = months,
                MembershipTrendData = membershipData,

                RecentFreezes = await _context.TblMemberShipFreezes
                    .Include(f => f.UserMemberShip)
                    .ThenInclude(um => um.User)
                    .Where(f => f.UserMemberShip.User.GymBranchId == gymBranchId)
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
                            UserName = f.UserMemberShip.User.UserName
                        }
                    })
                    .ToListAsync(),

                TotalRevenueThisMonth = await _context.TblUserMemberShips
                    .Include(um => um.MemberShipTypes)
                    .Include(um => um.User)
                    .Where(um => um.User.GymBranchId == gymBranchId &&
                                um.StartDate.Month == DateTime.Now.Month &&
                                um.StartDate.Year == DateTime.Now.Year)
                    .SumAsync(um => um.MemberShipTypes.Price)
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