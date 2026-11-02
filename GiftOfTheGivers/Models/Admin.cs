using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public DateTime ProvisionedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
