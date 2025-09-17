using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GYMappWeb.Models
{
    public class Checkin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CheckinId { get; set; }

        [Required]
        public DateTime CheckinDate { get; set; }

        [Required]
        public int UserId { get; set; }
            
        [Required]
        public int GymBranchId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual TblUser User { get; set; }

        [ForeignKey("GymBranchId")]
        public virtual GymBranch GymBranch { get; set; }

    }
}
