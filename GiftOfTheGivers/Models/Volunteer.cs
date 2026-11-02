using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models
{
    public class Volunteer
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        // Stored as a comma-separated list for simplicity; multi-select in the UI.
        [Required]
        public string Skills { get; set; } = string.Empty;

        [Required]
        public string Availability { get; set; } = string.Empty; // Weekdays, Weekends, Evenings, Flexible

        [Required]
        public string PreferredArea { get; set; } = string.Empty; // Emergency Relief, Food Security, Medical Support, Community Recovery

        public string? Experience { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        // Optional link if the volunteer also has a login account
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public VolunteerStatus Status { get; set; } = VolunteerStatus.Pending;
    }

    public enum VolunteerStatus
    {
        Pending,
        Approved,
        Declined
    }
}
