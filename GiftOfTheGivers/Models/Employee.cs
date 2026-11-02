using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public string Position { get; set; } = "Relief Coordinator";
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        public ICollection<ReliefProject> ProjectsCreated { get; set; } = new List<ReliefProject>();
        public ICollection<ProjectUpdate> ProjectUpdates { get; set; } = new List<ProjectUpdate>();
    }
}
