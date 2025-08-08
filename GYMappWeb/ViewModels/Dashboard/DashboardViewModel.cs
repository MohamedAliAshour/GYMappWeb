using GYMappWeb.ViewModels.TblMemberShipFreeze;

namespace GYMappWeb.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int TotalMembers { get; set; }
        public int ActiveMemberships { get; set; }
        public int ExpiringMemberships { get; set; }
        public List<TblMemberShipFreezeViewModel> RecentFreezes { get; set; }
    }
}
