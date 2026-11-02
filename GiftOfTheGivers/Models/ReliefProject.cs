using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Models
{
    public enum ProjectStatus
    {
        Planning,
        Upcoming,
        InProgress,
        Completed
    }

    public enum ProjectCategory
    {
        EmergencyRelief,
        FoodSecurity,
        MedicalSupport,
        CommunityRecovery
    }

    public class ReliefProject
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public ProjectCategory Category { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime ExpectedCompletionDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Budget { get; set; }

        [Required]
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        // 0-100
        public int ProgressPercentage { get; set; }

        public string? ImageUrl { get; set; }

        public int? CreatedByEmployeeId { get; set; }
        public Employee? CreatedByEmployee { get; set; }

        public ICollection<ProjectUpdate> Updates { get; set; } = new List<ProjectUpdate>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();

        [NotMapped]
        public string StatusLabel => Status switch
        {
            ProjectStatus.Planning => "Planning",
            ProjectStatus.Upcoming => "Upcoming",
            ProjectStatus.InProgress => "In Progress",
            ProjectStatus.Completed => "Completed",
            _ => Status.ToString()
        };

        [NotMapped]
        public string CategoryLabel => Category switch
        {
            ProjectCategory.EmergencyRelief => "Emergency Relief",
            ProjectCategory.FoodSecurity => "Food Security",
            ProjectCategory.MedicalSupport => "Medical Support",
            ProjectCategory.CommunityRecovery => "Community Recovery",
            _ => Category.ToString()
        };
    }
}
