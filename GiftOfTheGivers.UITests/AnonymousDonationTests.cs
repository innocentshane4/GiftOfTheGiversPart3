using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace GiftOfTheGivers.UITests
{
    // =========================================================================
    // Selenium Scenario 2 — Anonymous Donor Donation Flow
    //
    // Covers:
    //   1. Navigate to /Donations/Donate as an unauthenticated visitor
    //   2. Fill in donor name, email, amount, currency
    //   3. Check the "Make anonymous" checkbox
    //   4. Submit the form
    //   5. Verify the confirmation page displays the reference number
    // =========================================================================
    public class AnonymousDonationTests : SeleniumTestBase
    {
        // ------------------------------------------------------------------
        // Test 1: Anonymous donation form is accessible without login
        // ------------------------------------------------------------------
        [Fact]
        public void DonateForm_IsAccessibleWithoutLogin()
        {
            NavigateTo("/Donations/Donate");

            // The page title or heading should contain "Donate"
            var heading = WaitForElement(
                By.XPath("//*[self::h1 or self::h2][contains(text(),'Donat')]"));

            Assert.NotNull(heading);
        }

        // ------------------------------------------------------------------
        // Test 2: Submitting a valid anonymous donation redirects to
        //         the Confirmation page and shows a reference number
        // ------------------------------------------------------------------
        [Fact]
        public void AnonymousDonation_SubmitValidForm_ShowsConfirmation()
        {
            NavigateTo("/Donations/Donate");

            // Fill donor details
            FillField(By.Id("FullName"), "Anonymous Tester");
            FillField(By.Id("Email"),    "anon.test@example.com");

            // Select a custom amount
            FillField(By.Id("CustomAmount"), "250");

            // Select currency — ZAR is the default; leave as-is or pick explicitly
            var currencySelect = new SelectElement(Driver.FindElement(By.Id("Currency")));
            currencySelect.SelectByValue("ZAR");

            // Tick anonymous checkbox
            var anonymousCheckbox = Driver.FindElement(By.Id("IsAnonymous"));
            if (!anonymousCheckbox.Selected)
            {
                anonymousCheckbox.Click();
            }

            // Submit
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Confirm we landed on the confirmation page
            Assert.Contains("/Donations/Confirmation", Driver.Url,
                StringComparison.OrdinalIgnoreCase);

            // Confirmation page should display the reference number (GG-...)
            var body = Driver.FindElement(By.TagName("body")).Text;
            Assert.Contains("GG-", body, StringComparison.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------------
        // Test 3: Attempting a recurring + anonymous donation shows a
        //         validation error on the form
        // ------------------------------------------------------------------
        [Fact]
        public void RecurringAnonymousDonation_ShowsValidationError()
        {
            NavigateTo("/Donations/Donate");

            FillField(By.Id("FullName"), "Test User");
            FillField(By.Id("Email"),    "test@example.com");
            FillField(By.Id("CustomAmount"), "100");

            // Select Recurring donation type
            var typeSelect = new SelectElement(Driver.FindElement(By.Id("DonationType")));
            typeSelect.SelectByValue("Recurring");

            // Tick anonymous
            var cb = Driver.FindElement(By.Id("IsAnonymous"));
            if (!cb.Selected) cb.Click();

            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Should stay on the Donate page with a validation error
            Assert.Contains("/Donations/Donate", Driver.Url,
                StringComparison.OrdinalIgnoreCase);

            var body = Driver.FindElement(By.TagName("body")).Text;
            Assert.True(
                body.Contains("recurring", StringComparison.OrdinalIgnoreCase) &&
                body.Contains("anonymous", StringComparison.OrdinalIgnoreCase),
                "Expected a validation error about recurring anonymous donations.");
        }

        // ------------------------------------------------------------------
        // Test 4: The donation form shows the list of available projects
        // ------------------------------------------------------------------
        [Fact]
        public void DonateForm_ShowsAvailableProjects()
        {
            NavigateTo("/Donations/Donate");

            // The project dropdown must be present (even if it has no options,
            // it must exist — at minimum the "No preference" option)
            var projectDropdown = WaitForElement(By.Id("ReliefProjectId"));
            Assert.NotNull(projectDropdown);
        }
    }
}
