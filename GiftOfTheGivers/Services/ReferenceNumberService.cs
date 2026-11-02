namespace GiftOfTheGivers.Services
{
    /// <summary>
    /// Stand-in for the future custom C# NuGet helper package
    /// (donation reference formatting, tax certificate numbering, currency formatting).
    /// Kept as a single injectable service so it is a drop-in replacement later:
    /// swap the implementation for calls into the real NuGet package without
    /// touching any controller.
    /// </summary>
    public interface IReferenceNumberService
    {
        string GenerateDonationReference();
        string GenerateCertificateNumber();
        string FormatCurrency(decimal amount, string currencyCode);
    }

    public class ReferenceNumberService : IReferenceNumberService
    {
        public string GenerateDonationReference()
        {
            // GG-YYYYMMDD-XXXXXX
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Random.Shared.Next(100000, 999999);
            return $"GG-{datePart}-{randomPart}";
        }

        public string GenerateCertificateNumber()
        {
            var yearPart = DateTime.UtcNow.Year;
            var randomPart = Random.Shared.Next(1000, 9999);
            return $"GG-TC-{yearPart}-{randomPart}";
        }

        public string FormatCurrency(decimal amount, string currencyCode)
        {
            var symbol = currencyCode switch
            {
                "ZAR" => "R",
                "USD" => "$",
                "EUR" => "€",
                _ => currencyCode + " "
            };
            return $"{symbol}{amount:N2}";
        }
    }
}
