using Microsoft.AspNetCore.Identity;

namespace GiftOfTheGivers.Models
{
    /// <summary>
    /// Extends ASP.NET Core Identity's IdentityUser with the fields required
    /// by the Gift of the Givers domain (FullName, Phone, CreatedDate, Role).
    /// Role is also mirrored into an Identity Role claim/role for authorization,
    /// but is kept here too so it is easy to display and query.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Donor | Employee | Admin
        public string Role { get; set; } = UserRoles.Donor;

        public bool IsActive { get; set; } = true;

        // Navigation
        public Donor? Donor { get; set; }
        public Employee? Employee { get; set; }
        public Admin? Admin { get; set; }
    }

    public static class UserRoles
    {
        public const string Donor = "Donor";
        public const string Employee = "Employee";
        public const string Admin = "Admin";
    }
}
