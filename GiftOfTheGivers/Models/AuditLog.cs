using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Action { get; set; } = string.Empty; // e.g. "New donor registration"

        [Required, StringLength(200)]
        public string EntityName { get; set; } = string.Empty; // e.g. "Donor", "ReliefProject"

        public string? EntityId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; }
        public string? UserDisplayName { get; set; }

        [Required]
        public string Status { get; set; } = "Success"; // Success, Warning, Failed
    }
}
