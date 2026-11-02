using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.ViewModels
{
    public class VolunteerRegisterViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        [Display(Name = "Phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select at least one skill.")]
        [Display(Name = "Skills")]
        public List<string> Skills { get; set; } = new();

        [Required]
        [Display(Name = "Availability")]
        public string Availability { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Preferred area of support")]
        public string PreferredArea { get; set; } = string.Empty;

        [Display(Name = "Previous experience")]
        public string? Experience { get; set; }

        [Required(ErrorMessage = "You must accept the terms and conditions.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions.")]
        [Display(Name = "I agree to the terms and conditions")]
        public bool AcceptedTerms { get; set; }

        public static readonly string[] AvailableSkills =
        {
            "First Aid", "Logistics", "Driving", "Nursing", "Warehouse Management",
            "Community Outreach", "Counselling", "Administration", "Translation", "Fundraising"
        };

        public static readonly string[] AvailabilityOptions = { "Weekdays", "Weekends", "Evenings", "Flexible" };

        public static readonly string[] PreferredAreaOptions =
        {
            "Emergency Relief", "Food Security", "Medical Support", "Community Recovery"
        };
    }
}
