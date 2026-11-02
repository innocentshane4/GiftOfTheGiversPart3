using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Models
{
    public enum DonationType
    {
        OneTime,
        Recurring
    }

    public enum DonationStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum CurrencyCode
    {
        ZAR,
        USD,
        EUR
    }

    public class Donation
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(1, 10000000)]
        public decimal Amount { get; set; }

        [Required]
        public CurrencyCode Currency { get; set; } = CurrencyCode.ZAR;

        [Required]
        public DonationType DonationType { get; set; } = DonationType.OneTime;

        public bool IsAnonymous { get; set; }

        // Nullable so anonymous donations are not linked to a public donor account
        public int? DonorId { get; set; }
        public Donor? Donor { get; set; }

        // Optional donor-supplied name/email captured at time of donation
        public string? DonorFullName { get; set; }
        public string? DonorEmail { get; set; }

        public int? ReliefProjectId { get; set; }
        public ReliefProject? ReliefProject { get; set; }

        public DateTime DonationDate { get; set; } = DateTime.UtcNow;

        [Required, StringLength(30)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        public DonationStatus Status { get; set; } = DonationStatus.Completed;

        public TaxCertificate? TaxCertificate { get; set; }

        [NotMapped]
        public string StatusLabel => Status.ToString();
    }
}
