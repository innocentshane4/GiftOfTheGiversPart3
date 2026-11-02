using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GiftOfTheGivers.UITests
{
    /// <summary>
    /// Shared base class: spins up Chrome (headless) once per class, tears it
    /// down after all tests in that class complete.
    /// </summary>
    public abstract class SeleniumTestBase : IDisposable
    {
        // ---------------------------------------------------------------
        // Change this to the URL where you run the app during UI testing.
        // For local development:  https://localhost:7001
        // For Azure:              https://giftofthegivers.azurewebsites.net
        // ---------------------------------------------------------------
        protected const string BaseUrl = "https://localhost:7001";

        protected IWebDriver Driver { get; }

        protected SeleniumTestBase()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");   // headless Chrome (no visible window)
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--ignore-certificate-errors"); // allow dev HTTPS cert

            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Driver.Manage().Window.Size = new System.Drawing.Size(1366, 768);
        }

        public void Dispose()
        {
            Driver.Quit();
            Driver.Dispose();
        }

        // ---- Convenience helpers ----------------------------------------

        protected void NavigateTo(string relativePath)
            => Driver.Navigate().GoToUrl(BaseUrl + relativePath);

        protected IWebElement WaitForElement(By by, int timeoutSeconds = 10)
        {
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(
                Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                .ElementIsVisible(by));
        }

        protected void FillField(By by, string value)
        {
            var el = Driver.FindElement(by);
            el.Clear();
            el.SendKeys(value);
        }
    }
}
