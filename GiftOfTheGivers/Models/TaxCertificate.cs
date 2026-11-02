using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Models
{
    public class TaxCertificate
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string CertificateNumber { get; set; } = string.Empty;

        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public CurrencyCode Currency { get; set; } = CurrencyCode.ZAR;

        [Required]
        public int DonationId { get; set; }
        public Donation? Donation { get; set; }
    }
}
