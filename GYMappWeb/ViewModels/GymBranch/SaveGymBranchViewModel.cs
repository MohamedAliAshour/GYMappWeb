using System.ComponentModel.DataAnnotations;

namespace GYMappWeb.ViewModels.GymBranch
{
    public class SaveGymBranchViewModel
    {
        public int GymBranchId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GymName { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? CreatedBy { get; set; }
    }
}
