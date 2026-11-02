using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    [Authorize(Policy = "DonorOnly")]
    public class DonorDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReferenceNumberService _referenceNumberService;

        public DonorDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IReferenceNumberService referenceNumberService)
        {
            _context = context;
            _userManager = userManager;
            _referenceNumberService = referenceNumberService;
        }

        private async Task<Donor?> GetCurrentDonorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _context.Donors.Include(d => d.User).FirstOrDefaultAsync(d => d.UserId == user.Id);
        }

        public async Task<IActionResult> Index()
        {
            var donor = await GetCurrentDonorAsync();
            if (donor == null) return NotFound();

            var donations = await _context.Donations
                .Include(d => d.ReliefProject)
                .Include(d => d.TaxCertificate)
                .Where(d => d.DonorId == donor.Id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            var vm = new DonorDashboardViewModel
            {
                DonorName = donor.User?.FullName ?? "Donor",
                TotalDonations = donations.Sum(d => d.Amount),
                MostRecentDonation = donations.FirstOrDefault(),
                TaxCertificatesCount = donations.Count(d => d.TaxCertificate != null),
                RecentDonations = donations.Take(5).ToList()
            };

            ViewData["Title"] = "Dashboard";
            ViewData["ActiveNav"] = "dashboard";
            ViewData["DisplayName"] = vm.DonorName;
            return View(vm);
        }

        public async Task<IActionResult> MyDonations()
        {
            var donor = await GetCurrentDonorAsync();
            if (donor == null) return NotFound();

            var donations = await _context.Donations
                .Include(d => d.ReliefProject)
                .Include(d => d.TaxCertificate)
                .Where(d => d.DonorId == donor.Id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            ViewData["Title"] = "My Donations";
            ViewData["ActiveNav"] = "donations";
            ViewData["DisplayName"] = donor.User?.FullName;
            return View(donations);
        }

        public async Task<IActionResult> TaxCertificates()
        {
            var donor = await GetCurrentDonorAsync();
            if (donor == null) return NotFound();

            var donations = await _context.Donations
                .Include(d => d.TaxCertificate)
                .Include(d => d.ReliefProject)
                .Where(d => d.DonorId == donor.Id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            ViewData["Title"] = "Tax Certificates";
            ViewData["ActiveNav"] = "certificates";
            ViewData["DisplayName"] = donor.User?.FullName;
            return View(donations);
        }

        public async Task<IActionResult> Profile()
        {
            var donor = await GetCurrentDonorAsync();
            if (donor == null) return NotFound();

            ViewData["Title"] = "Profile";
            ViewData["ActiveNav"] = "profile";
            ViewData["DisplayName"] = donor.User?.FullName;
            return View(donor.User);
        }
    }
}
