using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models
{
    public class Donor
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<DonationSchedule> DonationSchedules { get; set; } = new List<DonationSchedule>();
    }
}
