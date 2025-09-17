using System.ComponentModel.DataAnnotations;

namespace GYMappWeb.ViewModels.Checkin
{
    public class SaveCheckinViewModel
    {
        public int CheckinId { get; set; }
        public DateTime CheckinDate { get; set; }
        public int UserId { get; set; }
        public string? GymBranchName { get; set; }
        public string? UserName { get; set; }
        public int? GymBranchId { get; set; }

        public string? CreatedBy { get; set; }
    }
}
