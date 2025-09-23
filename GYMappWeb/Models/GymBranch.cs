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

        public bool IsActive { get; set; }

        // Navigation properties
        public ICollection<ApplicationUser> ApplicationUsers { get; set; }
        public ICollection<TblUser> Users { get; set; }
        public ICollection<TblUserMemberShip> UserMemberships { get; set; }
        public ICollection<TblOffer> Offers { get; set; }
        public ICollection<TblMembershipType> MembershipTypes { get; set; }
        public ICollection<TblMemberShipFreeze> MembershipFreezes { get; set; }
        public ICollection<Checkin> Checkins { get; set; }
    }
}
