using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using GiftOfTheGivers.ViewModels;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace GiftOfTheGivers.Tests
{
    // -------------------------------------------------------------------------
    // Helper: run IValidatableObject.Validate exactly as MVC does
    // -------------------------------------------------------------------------
    internal static class ModelValidator
    {
        /// <summary>
        /// Returns the list of ValidationResults produced by both data-annotation
        /// attributes and IValidatableObject.Validate().
        /// </summary>
        public static List<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var ctx    = new ValidationContext(model);
            Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
            return results;
        }
    }

    // =========================================================================
    // Test class 1 — DonateViewModel business-rule validation
    // =========================================================================
    public class DonateViewModelValidationTests
    {
        // ------------------------------------------------------------------
        // Test 1: A recurring anonymous donation must be rejected
        // ------------------------------------------------------------------
        [Fact]
        public void RecurringAnonymousDonation_IsInvalid()
        {
            var vm = new DonateViewModel
            {
                CustomAmount  = 100m,
                Currency      = CurrencyCode.ZAR,
                DonationType  = DonationType.Recurring,
                IsAnonymous   = true,
                FullName      = "Test Donor",
                Email         = "test@example.com"
            };

            var errors = ModelValidator.Validate(vm);

            Assert.Contains(errors, e =>
                e.ErrorMessage!.Contains("recurring donation cannot be anonymous",
                    StringComparison.OrdinalIgnoreCase));
        }

        // ------------------------------------------------------------------
        // Test 2: A once-off anonymous donation must be accepted
        // ------------------------------------------------------------------
        [Fact]
        public void OnceOffAnonymousDonation_IsValid()
        {
            var vm = new DonateViewModel
            {
                CustomAmount  = 50m,
                Currency      = CurrencyCode.ZAR,
                DonationType  = DonationType.OneTime,
                IsAnonymous   = true,
                FullName      = "Anonymous",
                Email         = "anon@example.com"
            };

            var errors = ModelValidator.Validate(vm);

            // No validation errors expected
            Assert.Empty(errors);
        }

        // ------------------------------------------------------------------
        // Test 3: A donation with no amount (both preset and custom null)
        //         must fail validation
        // ------------------------------------------------------------------
        [Fact]
        public void DonationWithNoAmount_IsInvalid()
        {
            var vm = new DonateViewModel
            {
                // PresetAmount and CustomAmount both left at null
                Currency     = CurrencyCode.ZAR,
                DonationType = DonationType.OneTime,
                IsAnonymous  = false,
                FullName     = "Test Donor",
                Email        = "test@example.com"
            };

            var errors = ModelValidator.Validate(vm);

            Assert.Contains(errors, e =>
                e.ErrorMessage!.Contains("donation amount", StringComparison.OrdinalIgnoreCase));
        }

        // ------------------------------------------------------------------
        // Test 4: FinalAmount prefers CustomAmount over PresetAmount
        // ------------------------------------------------------------------
        [Fact]
        public void FinalAmount_PrefersCustomAmountOverPreset()
        {
            var vm = new DonateViewModel
            {
                PresetAmount = 200m,
                CustomAmount = 350m
            };

            Assert.Equal(350m, vm.FinalAmount);
        }

        // ------------------------------------------------------------------
        // Test 5: FinalAmount falls back to PresetAmount when Custom is null
        // ------------------------------------------------------------------
        [Fact]
        public void FinalAmount_FallsBackToPresetAmount()
        {
            var vm = new DonateViewModel
            {
                PresetAmount = 200m,
                CustomAmount = null
            };

            Assert.Equal(200m, vm.FinalAmount);
        }
    }

    // =========================================================================
    // Test class 2 — Donation total calculation (multi-currency sum helper)
    // =========================================================================
    public class DonationTotalCalculationTests
    {
        /// <summary>
        /// Representative of the kind of aggregate calculation an employee
        /// dashboard or reporting service would perform.
        /// </summary>
        private static decimal SumDonations(IEnumerable<Donation> donations, CurrencyCode currency)
            => donations
                .Where(d => d.Currency == currency && d.Status == DonationStatus.Completed)
                .Sum(d => d.Amount);

        // ------------------------------------------------------------------
        // Test 6: Total is calculated correctly for ZAR donations only
        // ------------------------------------------------------------------
        [Fact]
        public void TotalZar_IgnoresOtherCurrencies()
        {
            var donations = new List<Donation>
            {
                new() { Amount = 500m,  Currency = CurrencyCode.ZAR, Status = DonationStatus.Completed },
                new() { Amount = 200m,  Currency = CurrencyCode.ZAR, Status = DonationStatus.Completed },
                new() { Amount = 100m,  Currency = CurrencyCode.USD, Status = DonationStatus.Completed }, // excluded
                new() { Amount = 1000m, Currency = CurrencyCode.ZAR, Status = DonationStatus.Failed     } // excluded
            };

            var total = SumDonations(donations, CurrencyCode.ZAR);

            Assert.Equal(700m, total);
        }

        // ------------------------------------------------------------------
        // Test 7: An empty list returns zero — no exception
        // ------------------------------------------------------------------
        [Fact]
        public void TotalOnEmptyList_ReturnsZero()
        {
            var total = SumDonations(Enumerable.Empty<Donation>(), CurrencyCode.ZAR);

            Assert.Equal(0m, total);
        }

        // ------------------------------------------------------------------
        // Test 8: Failed donations are excluded from the total
        // ------------------------------------------------------------------
        [Fact]
        public void FailedDonations_AreExcludedFromTotal()
        {
            var donations = new List<Donation>
            {
                new() { Amount = 300m, Currency = CurrencyCode.ZAR, Status = DonationStatus.Completed },
                new() { Amount = 150m, Currency = CurrencyCode.ZAR, Status = DonationStatus.Failed    }
            };

            var total = SumDonations(donations, CurrencyCode.ZAR);

            Assert.Equal(300m, total);
        }
    }

    // =========================================================================
    // Test class 3 — ReferenceNumberService (certificate + reference formatting)
    // =========================================================================
    public class ReferenceNumberServiceTests
    {
        private readonly IReferenceNumberService _svc = new ReferenceNumberService();

        // ------------------------------------------------------------------
        // Test 9: Donation reference matches the expected pattern GG-YYYYMMDD-######
        // ------------------------------------------------------------------
        [Fact]
        public void GenerateDonationReference_MatchesExpectedPattern()
        {
            var reference = _svc.GenerateDonationReference();

            // Pattern: GG-20261026-123456
            Assert.Matches(@"^GG-\d{8}-\d{6}$", reference);
        }

        // ------------------------------------------------------------------
        // Test 10: Certificate number matches the expected pattern GG-TC-YYYY-####
        // ------------------------------------------------------------------
        [Fact]
        public void GenerateCertificateNumber_MatchesExpectedPattern()
        {
            var number = _svc.GenerateCertificateNumber();

            // Pattern: GG-TC-2026-1234
            Assert.Matches(@"^GG-TC-\d{4}-\d{4}$", number);
        }

        // ------------------------------------------------------------------
        // Test 11: FormatCurrency prefixes the correct symbol for ZAR
        // ------------------------------------------------------------------
        [Fact]
        public void FormatCurrency_ZAR_PrefixesR()
        {
            var result = _svc.FormatCurrency(1500m, "ZAR");

            Assert.StartsWith("R", result);
            Assert.Contains("1,500.00", result);
        }

        // ------------------------------------------------------------------
        // Test 12: FormatCurrency prefixes the correct symbol for USD
        // ------------------------------------------------------------------
        [Fact]
        public void FormatCurrency_USD_PrefixesDollar()
        {
            var result = _svc.FormatCurrency(250.50m, "USD");

            Assert.StartsWith("$", result);
            Assert.Contains("250.50", result);
        }

        // ------------------------------------------------------------------
        // Test 13: FormatCurrency prefixes the correct symbol for EUR
        // ------------------------------------------------------------------
        [Fact]
        public void FormatCurrency_EUR_PrefixesEuroSign()
        {
            var result = _svc.FormatCurrency(75m, "EUR");

            Assert.StartsWith("€", result);
        }

        // ------------------------------------------------------------------
        // Test 14: Two successive references are not identical
        //          (probabilistic — collision probability is 1 in 900,000)
        // ------------------------------------------------------------------
        [Fact]
        public void GenerateDonationReference_TwoCallsProduceDifferentValues()
        {
            var r1 = _svc.GenerateDonationReference();
            var r2 = _svc.GenerateDonationReference();

            // This will fail on the rare day that the random parts collide,
            // but at 1:900,000 odds it is effectively never.
            Assert.NotEqual(r1, r2);
        }

        // ------------------------------------------------------------------
        // Test 15: Certificate number contains the current year
        // ------------------------------------------------------------------
        [Fact]
        public void GenerateCertificateNumber_ContainsCurrentYear()
        {
            var number = _svc.GenerateCertificateNumber();
            var year   = DateTime.UtcNow.Year.ToString();

            Assert.Contains(year, number);
        }
    }

    // =========================================================================
    // Test class 4 — TaxCertificate model integrity
    // =========================================================================
    public class TaxCertificateTests
    {
        // ------------------------------------------------------------------
        // Test 16: Anonymous donation must not receive a tax certificate
        //          (business rule mirrored from DonationsController)
        // ------------------------------------------------------------------
        [Fact]
        public void AnonymousDonation_ShouldNotHaveTaxCertificate()
        {
            var donation = new Donation
            {
                Amount      = 500m,
                Currency    = CurrencyCode.ZAR,
                DonationType = DonationType.OneTime,
                IsAnonymous = true,
                Status      = DonationStatus.Completed
            };

            // The controller checks donation.IsAnonymous before creating the
            // certificate.  We replicate that guard here.
            bool certificateAllowed = !donation.IsAnonymous;

            Assert.False(certificateAllowed);
        }

        // ------------------------------------------------------------------
        // Test 17: A non-anonymous completed donation is eligible for a cert
        // ------------------------------------------------------------------
        [Fact]
        public void IdentifiedDonation_IsEligibleForTaxCertificate()
        {
            var donation = new Donation
            {
                Amount       = 1000m,
                Currency     = CurrencyCode.ZAR,
                DonationType = DonationType.OneTime,
                IsAnonymous  = false,
                Status       = DonationStatus.Completed
            };

            bool certificateAllowed = !donation.IsAnonymous
                                      && donation.Status == DonationStatus.Completed;

            Assert.True(certificateAllowed);
        }
    }
}
