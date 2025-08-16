using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblUserMemberShip;

namespace GYMappWeb.ViewModels.TblOffer
{
    public class GetWithPaginationTblOfferViewModel
    {
        public int OffId { get; set; }

        public string OfferName { get; set; } = null!;
        public int DiscountPrecentage { get; set; }
        public int MemberShipTypesId { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual TblMembershipType? MemberShipTypes { get; set; } = null!;
        public virtual ICollection<TblUserMemberShipViewModel>? TblUserMemberShips { get; set; } = new List<TblUserMemberShipViewModel>();
    }
}
