using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using GYMappWeb.Areas.Identity.Data;

namespace GYMappWeb.Models
{
    public class GymBranch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GymBranchId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GymName { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; }

        [Required]
        public DateTime CreateDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<TblUser> Users { get; set; }
        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }
        public virtual ICollection<Checkin> Checkins { get; set; }
    }
}
