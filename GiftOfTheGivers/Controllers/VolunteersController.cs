using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Controllers
{
    public class VolunteersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VolunteersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new VolunteerRegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(VolunteerRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var volunteer = new Volunteer
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Skills = string.Join(", ", model.Skills),
                Availability = model.Availability,
                PreferredArea = model.PreferredArea,
                Experience = model.Experience,
                RegistrationDate = DateTime.UtcNow,
                Status = VolunteerStatus.Pending
            };

            _context.Volunteers.Add(volunteer);
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "Volunteer registration submitted",
                EntityName = "Volunteer",
                UserDisplayName = model.FullName,
                Status = "Success"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you! Your volunteer application has been received. Our team will be in touch soon.";
            return RedirectToAction("Register");
        }
    }
}
