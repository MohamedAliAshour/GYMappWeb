using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.ViewModels.TblUser;

namespace GYMappWeb.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int TodaysCheckins { get; set; }
        public int ActiveMemberships { get; set; }
        public int ExpiringMemberships { get; set; }
        public int NewMembersThisMonth { get; set; }
        public int TotalMembers { get; set; }

        // Add these new properties for percentages
        public int MaxCheckins { get; set; }
        public double CheckinPercentage { get; set; }
        public double ActiveMembershipPercentage { get; set; }
        public double ExpiringPercentage { get; set; }
        public double NewMembersPercentage { get; set; }

        // Membership trends data
        public List<string> MembershipTrendMonths { get; set; }
        public List<int> MembershipTrendData { get; set; }

        // Rest of your properties...
        public int ActiveOffers { get; set; }
        public decimal TotalRevenueThisMonth { get; set; }
        public List<TblMemberShipFreezeViewModel> RecentFreezes { get; set; }
        public List<TblUserViewModel> AllUsers { get; set; }
    }
}
