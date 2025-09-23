namespace GYMappWeb.ViewModels.GymBranch
{
    public class GetGymBranchViewModel
    {
        public int GymBranchId { get; set; }
        public string GymName { get; set; }
        public string Location { get; set; }
        public DateTime CreateDate { get; set; }
        // In GymBranch model
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByUserName { get; set; }
    }
}
