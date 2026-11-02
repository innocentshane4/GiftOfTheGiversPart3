
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using Xunit;

namespace GiftOfTheGivers.SeleniumTests
{
    public class GiftOfTheGiversTests : IDisposable
    {
        private readonly IWebDriver driver;
        private readonly string baseUrl = "https://localhost:7099/";

        public GiftOfTheGiversTests()
        {
            var options = new EdgeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--ignore-certificate-errors");

            driver = new EdgeDriver(options);
            driver.Manage().Timeouts().ImplicitWait =
                TimeSpan.FromSeconds(5);
        }

        [Fact]
        public void Homepage_ShouldLoadSuccessfully()
        {
            driver.Navigate().GoToUrl(baseUrl);

            Assert.Contains(
                "Gift",
                driver.Title,
                StringComparison.OrdinalIgnoreCase
            );

            Assert.True(driver.FindElement(By.TagName("body")).Displayed);
        }

        [Fact]
        public void Homepage_ShouldHaveNavigation()
        {
            driver.Navigate().GoToUrl(baseUrl);

            var navigation = driver.FindElements(By.TagName("nav"));

            Assert.NotEmpty(navigation);
        }

        [Fact]
        public void Website_ShouldDisplayPageContent()
        {
            driver.Navigate().GoToUrl(baseUrl);

            var bodyText = driver.FindElement(By.TagName("body")).Text;

            Assert.False(string.IsNullOrWhiteSpace(bodyText));
        }

        [Fact]
        public void LoginPage_ShouldLoad()
        {
            driver.Navigate().GoToUrl(
                new Uri(new Uri(baseUrl), "Account/Login").ToString()
            );

            Assert.Equal(200, driver.FindElement(By.TagName("body"))
                .Displayed ? 200 : 0);
        }

        [Fact]
        public void Website_ShouldUseHttps()
        {
            driver.Navigate().GoToUrl(baseUrl);

            Assert.StartsWith(
                "https://",
                driver.Url,
                StringComparison.OrdinalIgnoreCase
            );
        }

        public void Dispose()
        {
            driver.Quit();
            driver.Dispose();
        }
    }
}
