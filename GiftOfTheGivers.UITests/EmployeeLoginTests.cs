using OpenQA.Selenium;
using Xunit;

namespace GiftOfTheGivers.UITests
{
    // =========================================================================
    // Selenium Scenario 1 — Employee Login
    //
    // Covers:
    //   1. Navigate to /Account/Login
    //   2. Enter seeded employee credentials
    //   3. Submit the form
    //   4. Verify the employee dashboard is displayed
    //   5. Verify a logout link is present
    // =========================================================================
    public class EmployeeLoginTests : SeleniumTestBase
    {
        // Credentials that match the seeded employee in DbInitializer
        private const string EmployeeEmail    = "employee@gotg.org";
        private const string EmployeePassword = "Employee@123";

        // ------------------------------------------------------------------
        // Test 1: Successful employee login lands on the employee dashboard
        // ------------------------------------------------------------------
        [Fact]
        public void EmployeeLogin_ValidCredentials_RedirectsToDashboard()
        {
            NavigateTo("/Account/Login");

            // Locate and fill the login form
            FillField(By.Id("Input_Email"),    EmployeeEmail);
            FillField(By.Id("Input_Password"), EmployeePassword);
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // After login, the URL should contain /EmployeeDashboard
            Assert.Contains("/EmployeeDashboard", Driver.Url,
                StringComparison.OrdinalIgnoreCase);

            // The page should include "Welcome" or "Employee" heading
            var body = Driver.FindElement(By.TagName("body")).Text;
            Assert.True(
                body.Contains("Employee", StringComparison.OrdinalIgnoreCase) ||
                body.Contains("Dashboard", StringComparison.OrdinalIgnoreCase),
                $"Expected dashboard content. Page body was:\n{body[..Math.Min(500, body.Length)]}");
        }

        // ------------------------------------------------------------------
        // Test 2: A logout link is present after login
        // ------------------------------------------------------------------
        [Fact]
        public void EmployeeLogin_AfterLogin_LogoutLinkIsVisible()
        {
            NavigateTo("/Account/Login");

            FillField(By.Id("Input_Email"),    EmployeeEmail);
            FillField(By.Id("Input_Password"), EmployeePassword);
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // The shared layout renders a logout link for authenticated users
            var logoutEl = WaitForElement(
                By.XPath("//*[contains(text(),'Logout') or contains(text(),'Log out')]"));

            Assert.NotNull(logoutEl);
        }

        // ------------------------------------------------------------------
        // Test 3: Invalid credentials show an error message
        // ------------------------------------------------------------------
        [Fact]
        public void EmployeeLogin_InvalidCredentials_ShowsErrorMessage()
        {
            NavigateTo("/Account/Login");

            FillField(By.Id("Input_Email"),    "wrong@example.com");
            FillField(By.Id("Input_Password"), "WrongPassword123!");
            Driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // MVC renders validation summary or field-level error
            var body = Driver.FindElement(By.TagName("body")).Text;
            Assert.True(
                body.Contains("Invalid", StringComparison.OrdinalIgnoreCase) ||
                body.Contains("incorrect", StringComparison.OrdinalIgnoreCase) ||
                body.Contains("failed", StringComparison.OrdinalIgnoreCase),
                "Expected an error message for invalid credentials.");
        }
    }
}
