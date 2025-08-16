using GYMappWeb.ViewModels.TblUserMemberShip;

namespace GYMappWeb.ViewModels.TblMemberShipFreeze
{
    public class SaveTblMemberShipFreezeViewModel
    {
        public int MemberShipFreezeId { get; set; }

        public int UserMemberShipId { get; set; }
        public DateOnly FreezeStartDate { get; set; }
        public DateOnly FreezeEndDate { get; set; }
        public string? Reason { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual TblUserMemberShipViewModel? UserMemberShip { get; set; } = null!;
    }
}
