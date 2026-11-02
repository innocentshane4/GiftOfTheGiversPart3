using GiftOfTheGivers.Controllers;
using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GiftOfTheGivers.Tests.Integration
{
    // =========================================================================
    // Integration Tests — Full donation flow:
    //   HTTP POST  →  DonationsController.Donate()
    //              →  ApplicationDbContext (InMemory)
    //              →  Confirmation redirect
    //
    // These tests exercise the real controller, the real service, and a real
    // (in-process) EF Core InMemory database.  The only stub is UserManager,
    // which requires ASP.NET Core Identity infrastructure that is not worth
    // spinning up in an isolated test.
    // =========================================================================
    public class DonationFlowIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext   _db;
        private readonly IReferenceNumberService _refSvc;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly DonationsController    _controller;

        public DonationFlowIntegrationTests()
        {
            // ---- EF Core InMemory database (unique per test run) -----------
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db     = new ApplicationDbContext(options);
            _refSvc = new ReferenceNumberService();

            // ---- Seed a ReliefProject so the controller FK is satisfied ----
            _db.ReliefProjects.Add(new ReliefProject
            {
                Name                   = "Flood Relief 2026",
                Description            = "Emergency flood assistance",
                Location               = "KwaZulu-Natal",
                Category               = ProjectCategory.EmergencyRelief,
                StartDate              = DateTime.UtcNow.AddMonths(-1),
                ExpectedCompletionDate = DateTime.UtcNow.AddMonths(3),
                Status                 = ProjectStatus.InProgress
            });
            _db.SaveChanges();

            // ---- Minimal UserManager stub ----------------------------------
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock  = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // ---- Controller wired to InMemory db ---------------------------
            _controller = new DonationsController(_db, _userManagerMock.Object, _refSvc);

            // Provide an unauthenticated HttpContext so User.Identity is set
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // ------------------------------------------------------------------
        // Integration Test 1:
        //   An anonymous one-off donation submitted via POST is persisted to
        //   the database and the controller redirects to Confirmation.
        // ------------------------------------------------------------------
        [Fact]
        public async Task AnonymousDonation_IsSavedAndRedirectsToConfirmation()
        {
            var model = new DonateViewModel
            {
                CustomAmount = 200m,
                Currency     = CurrencyCode.ZAR,
                DonationType = DonationType.OneTime,
                IsAnonymous  = true,
                FullName     = "Anonymous Donor",
                Email        = "anon@example.com"
            };

            var result = await _controller.Donate(model);

            // Assert: controller returns a redirect
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Confirmation", redirect.ActionName);

            // Assert: exactly one donation in the database
            Assert.Equal(1, await _db.Donations.CountAsync());

            var savedDonation = await _db.Donations.FirstAsync();
            Assert.Equal(200m,                savedDonation.Amount);
            Assert.Equal(CurrencyCode.ZAR,    savedDonation.Currency);
            Assert.True(savedDonation.IsAnonymous);
            Assert.Null(savedDonation.DonorId);
            Assert.Null(savedDonation.DonorFullName);   // anonymised: name must not be stored
            Assert.NotEmpty(savedDonation.ReferenceNumber);
        }

        // ------------------------------------------------------------------
        // Integration Test 2:
        //   A donation with an amount of 0 (no preset, no custom) is rejected
        //   by model validation before it touches the database.
        // ------------------------------------------------------------------
        [Fact]
        public async Task DonationWithZeroAmount_FailsModelValidationAndDoesNotPersist()
        {
            var model = new DonateViewModel
            {
                // Both PresetAmount and CustomAmount left null — FinalAmount = 0
                Currency     = CurrencyCode.ZAR,
                DonationType = DonationType.OneTime,
                IsAnonymous  = false,
                FullName     = "Test Donor",
                Email        = "test@example.com"
            };

            // Simulate MVC model-binding validation
            _controller.ModelState.AddModelError(
                nameof(DonateViewModel.CustomAmount),
                "Please select a preset amount or enter a custom donation amount.");

            var result = await _controller.Donate(model);

            // Assert: controller returns the form view, not a redirect
            Assert.IsType<ViewResult>(result);

            // Assert: nothing was written to the database
            Assert.Equal(0, await _db.Donations.CountAsync());
        }

        // ------------------------------------------------------------------
        // Integration Test 3:
        //   Submitting a donation generates a reference number that is stored
        //   and matches the expected GG-YYYYMMDD-###### pattern.
        // ------------------------------------------------------------------
        [Fact]
        public async Task SubmittedDonation_HasWellFormedReferenceNumber()
        {
            var model = new DonateViewModel
            {
                CustomAmount = 500m,
                Currency     = CurrencyCode.EUR,
                DonationType = DonationType.OneTime,
                IsAnonymous  = false,
                FullName     = "Jean Dupont",
                Email        = "jean@example.com"
            };

            await _controller.Donate(model);

            var saved = await _db.Donations.FirstAsync();
            Assert.Matches(@"^GG-\d{8}-\d{6}$", saved.ReferenceNumber);
        }

        // ------------------------------------------------------------------
        // Integration Test 4:
        //   A donation linked to a specific ReliefProject stores the FK.
        // ------------------------------------------------------------------
        [Fact]
        public async Task DonationLinkedToProject_StoredProjectForeignKey()
        {
            var model = new DonateViewModel
            {
                CustomAmount    = 750m,
                Currency        = CurrencyCode.ZAR,
                DonationType    = DonationType.OneTime,
                IsAnonymous     = false,
                FullName        = "Sipho Dlamini",
                Email           = "sipho@example.com",
                ReliefProjectId = 1
            };

            await _controller.Donate(model);

            var saved = await _db.Donations.FirstAsync();
            Assert.Equal(1, saved.ReliefProjectId);
        }

        // ------------------------------------------------------------------
        // Integration Test 5:
        //   A recurring + anonymous donation (invalid combination) is caught
        //   when the controller manually validates IValidatableObject.
        //   We test this by pre-populating ModelState with the error that MVC
        //   would add after binding, verifying the controller returns the View.
        // ------------------------------------------------------------------
        [Fact]
        public async Task RecurringAnonymousDonation_IsRejectedByController()
        {
            var model = new DonateViewModel
            {
                CustomAmount = 300m,
                Currency     = CurrencyCode.ZAR,
                DonationType = DonationType.Recurring,
                IsAnonymous  = true,
                FullName     = "Anonymous",
                Email        = "anon@example.com"
            };

            _controller.ModelState.AddModelError(
                nameof(DonateViewModel.IsAnonymous),
                "A recurring donation cannot be anonymous.");

            var result = await _controller.Donate(model);

            // Controller must return the form, not redirect
            Assert.IsType<ViewResult>(result);
            Assert.Equal(0, await _db.Donations.CountAsync());
        }

        public void Dispose() => _db.Dispose();
    }
}
