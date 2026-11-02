using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.Services;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    public class DonationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReferenceNumberService _referenceNumberService;

        public DonationsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IReferenceNumberService referenceNumberService)
        {
            _context = context;
            _userManager = userManager;
            _referenceNumberService = referenceNumberService;
        }

        [HttpGet]
        public async Task<IActionResult> Donate(int? projectId)
        {
            var vm = new DonateViewModel
            {
                ReliefProjectId = projectId,
                AvailableProjects = await _context.ReliefProjects.OrderBy(p => p.Name).ToListAsync()
            };

            // Pre-fill name/email for a logged-in donor for convenience.
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    vm.FullName = user.FullName;
                    vm.Email = user.Email ?? string.Empty;
                }
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(DonateViewModel model)
        {
            model.AvailableProjects = await _context.ReliefProjects.OrderBy(p => p.Name).ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Donor? donor = null;
            if (User.Identity?.IsAuthenticated == true && !model.IsAnonymous)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    donor = await _context.Donors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                }
            }

            var donation = new Donation
            {
                Amount = model.FinalAmount,
                Currency = model.Currency,
                DonationType = model.DonationType,
                IsAnonymous = model.IsAnonymous,
                // Anonymous donations must never be linked to a public donor account.
                DonorId = model.IsAnonymous ? null : donor?.Id,
                DonorFullName = model.IsAnonymous ? null : model.FullName,
                DonorEmail = model.IsAnonymous ? null : model.Email,
                ReliefProjectId = model.ReliefProjectId,
                DonationDate = DateTime.UtcNow,
                ReferenceNumber = _referenceNumberService.GenerateDonationReference(),
                Status = DonationStatus.Completed // Prototype: no real payment gateway, so it's marked completed immediately.
            };

            _context.Donations.Add(donation);

            _context.AuditLogs.Add(new AuditLog
            {
                Action = "Donation recorded",
                EntityName = "Donation",
                EntityId = null,
                UserId = donor?.UserId,
                UserDisplayName = model.IsAnonymous ? "Anonymous Donor" : model.FullName,
                Status = "Success"
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation", new { id = donation.Id });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.ReliefProject)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        public async Task<IActionResult> TaxCertificate(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.Donor).ThenInclude(d => d!.User)
                .Include(d => d.TaxCertificate)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donation == null)
            {
                return NotFound();
            }

            if (donation.IsAnonymous)
            {
                // Anonymous donations do not receive donor-identified tax certificates.
                return BadRequest("A tax certificate is not available for an anonymous donation.");
            }

            // A tax certificate is generated once per donation and not regenerated on
            // every request, per the spec.
            if (donation.TaxCertificate == null)
            {
                var certificate = new TaxCertificate
                {
                    CertificateNumber = _referenceNumberService.GenerateCertificateNumber(),
                    IssuedDate = DateTime.UtcNow,
                    Amount = donation.Amount,
                    Currency = donation.Currency,
                    DonationId = donation.Id
                };
                _context.TaxCertificates.Add(certificate);
                await _context.SaveChangesAsync();
                donation.TaxCertificate = certificate;
            }

            return View(donation);
        }
    }
}
