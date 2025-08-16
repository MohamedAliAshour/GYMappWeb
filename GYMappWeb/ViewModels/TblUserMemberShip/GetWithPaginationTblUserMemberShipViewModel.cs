using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.ViewModels.TblMemberShipType;
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.ViewModels.TblUser;

namespace GYMappWeb.ViewModels.TblUserMemberShip
{
    public class GetWithPaginationTblUserMemberShipViewModel
    {
        public int UserMemberShipId { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsActive { get; set; }
        public int? invitationUsed { get; set; }
        public int? TotalFreezedDays { get; set; }
        public int? OffId { get; set; }
        public int UserId { get; set; }
        public int MemberShipTypesId { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? UserName { get; set; }
        public virtual TblMemberShipTypeViewModel? MemberShipTypes { get; set; } = null!;
        public virtual TblOfferViewModel? Off { get; set; }
        public virtual ICollection<TblMemberShipFreezeViewModel> TblMemberShipFreezes { get; set; } = new List<TblMemberShipFreezeViewModel>();
        public virtual TblUserViewModel? User { get; set; } = null!;
    }
}
