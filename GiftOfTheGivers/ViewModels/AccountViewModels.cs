using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class AdminLoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Admin username or email")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Public self-registration is Donor-only. Role is never exposed as a
        // selectable field in the UI or bound from the form on purpose.
    }
}
