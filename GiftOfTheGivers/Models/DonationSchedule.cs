using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Models
{
    public enum ScheduleFrequency
    {
        Weekly,
        Monthly,
        Quarterly,
        Yearly
    }

    public class DonationSchedule
    {
        public int Id { get; set; }

        [Required]
        public int DonorId { get; set; }
        public Donor? Donor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public CurrencyCode Currency { get; set; } = CurrencyCode.ZAR;

        [Required]
        public ScheduleFrequency Frequency { get; set; } = ScheduleFrequency.Monthly;

        public DateTime NextRunDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        public bool IsActive { get; set; } = true;
    }
}
