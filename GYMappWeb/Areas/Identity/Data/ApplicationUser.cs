using Microsoft.AspNetCore.Identity;

namespace GYMappWeb.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public int? GymBranchId { get; set; }
    }
}
