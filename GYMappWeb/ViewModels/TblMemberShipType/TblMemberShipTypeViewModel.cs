
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.ViewModels.TblUserMemberShip;

namespace GYMappWeb.ViewModels.TblMemberShipType
{
    public class TblMemberShipTypeViewModel
    {
        public int MemberShipTypesId { get; set; }

        public string Name { get; set; } = null!;
        public int MembershipDuration { get; set; }
        public int Price { get; set; }
        public int invitationCount { get; set; }
        public string? Description { get; set; }
        public int FreezeCount { get; set; }
        public bool IsActive { get; set; }
        public int TotalFreezeDays { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByUserName { get; set; }
        public int? GymBranchId { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ICollection<TblOfferViewModel> TblOffers { get; set; } = new List<TblOfferViewModel>();
        public virtual ICollection<TblUserMemberShipViewModel> TblUserMemberShips { get; set; } = new List<TblUserMemberShipViewModel>();
    }
}
