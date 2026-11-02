using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.ViewModels
{
    /// <summary>
    /// Used by Admins to internally provision Employee or Admin accounts.
    /// This is the ONLY place in the application where an Employee or Admin
    /// account can be created — never through public registration.
    /// </summary>
    public class ProvisionStaffUserViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
        [Display(Name = "Temporary password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = Models.UserRoles.Employee; // Employee or Admin only
    }
}
