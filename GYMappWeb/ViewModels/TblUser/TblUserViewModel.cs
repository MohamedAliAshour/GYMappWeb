using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblUserMemberShip;

namespace GYMappWeb.ViewModels.TblUser
{
    public class TblUserViewModel
    {
        public int UserId { get; set; }

        public int UserCode { get; set; }
        public string UserName { get; set; } = null!;
        public string UserPhone { get; set; } = null!;
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MembershipName { get; set; } // Add this
        public string? CreatedBy { get; set; }
        public int? GymBranchId { get; set; }
        public string? CreatedByUserName { get; set; }
        public virtual ICollection<TblUserMemberShipViewModel> TblUserMemberShips { get; set; } = new List<TblUserMemberShipViewModel>();
    }
}
