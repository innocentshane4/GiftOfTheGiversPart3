using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Models;

namespace GiftOfTheGivers.ViewModels
{
    /// <summary>
    /// IValidatableObject enforces the business rule from the spec:
    /// a recurring donation cannot be anonymous because a recurring donation
    /// requires an identified donor.
    /// </summary>
    public class DonateViewModel : IValidatableObject
    {
        public decimal? PresetAmount { get; set; }

        [Range(10, 10000000, ErrorMessage = "Please enter a donation amount of at least R10.")]
        public decimal? CustomAmount { get; set; }

        [Required]
        public CurrencyCode Currency { get; set; } = CurrencyCode.ZAR;

        [Required]
        public DonationType DonationType { get; set; } = DonationType.OneTime;

        [Required(ErrorMessage = "Please enter your full name.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Make this donation anonymous")]
        public bool IsAnonymous { get; set; }

        [Display(Name = "Relief project (optional)")]
        public int? ReliefProjectId { get; set; }

        public List<ReliefProject> AvailableProjects { get; set; } = new();

        public decimal FinalAmount => CustomAmount ?? PresetAmount ?? 0m;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DonationType == DonationType.Recurring && IsAnonymous)
            {
                yield return new ValidationResult(
                    "A recurring donation cannot be anonymous because a recurring donation requires an identified donor.",
                    new[] { nameof(IsAnonymous), nameof(DonationType) });
            }

            if (FinalAmount <= 0)
            {
                yield return new ValidationResult(
                    "Please select a preset amount or enter a custom donation amount.",
                    new[] { nameof(CustomAmount) });
            }
        }
    }
}
