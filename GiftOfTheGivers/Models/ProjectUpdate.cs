using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models
{
    public class ProjectUpdate
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int ReliefProjectId { get; set; }
        public ReliefProject? ReliefProject { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
    }
}
