using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<string?> CurrentDisplayNameAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.FullName;
        }

        public async Task<IActionResult> Index()
        {
            // SQLite's EF Core provider cannot translate SUM() over a decimal column into SQL,
            // so the amounts are pulled back first and summed in memory. Fine for our seeded
            // demo dataset; on SQL Server/Azure SQL this could be pushed back into SumAsync().
            var donationAmounts = await _context.Donations.Select(d => d.Amount).ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalDonationValue = donationAmounts.Sum(),
                TotalProjects = await _context.ReliefProjects.CountAsync(),
                ActiveVolunteers = await _context.Volunteers.CountAsync(v => v.Status == VolunteerStatus.Approved),
                RecentActivity = await _context.AuditLogs.OrderByDescending(a => a.Timestamp).Take(10).ToListAsync()
            };

            ViewData["Title"] = "System Overview and Management";
            ViewData["ActiveNav"] = "dashboard";
            ViewData["DisplayName"] = await CurrentDisplayNameAsync();
            return View(vm);
        }

        // ---------------- User Management ----------------

        public async Task<IActionResult> UserManagement()
        {
            ViewData["Title"] = "User Management";
            ViewData["ActiveNav"] = "users";
            ViewData["DisplayName"] = await CurrentDisplayNameAsync();

            var users = await _context.Users.OrderByDescending(u => u.CreatedDate).ToListAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult ProvisionStaffUser()
        {
            ViewData["Title"] = "Provision Staff Account";
            ViewData["ActiveNav"] = "users";
            return View(new ProvisionStaffUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProvisionStaffUser(ProvisionStaffUserViewModel model)
        {
            // Defence in depth: only Employee or Admin can be provisioned here.
            if (model.Role != UserRoles.Employee && model.Role != UserRoles.Admin)
            {
                ModelState.AddModelError(nameof(model.Role), "Only Employee or Admin roles can be provisioned.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Provision Staff Account";
                ViewData["ActiveNav"] = "users";
                return View(model);
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                ViewData["Title"] = "Provision Staff Account";
                ViewData["ActiveNav"] = "users";
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FullName = model.FullName,
                Role = model.Role,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                ViewData["Title"] = "Provision Staff Account";
                ViewData["ActiveNav"] = "users";
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            if (model.Role == UserRoles.Employee)
            {
                _context.Employees.Add(new Employee { UserId = user.Id, HireDate = DateTime.UtcNow });
            }
            else
            {
                _context.Admins.Add(new Admin { UserId = user.Id, ProvisionedDate = DateTime.UtcNow });
            }

            var adminUser = await _userManager.GetUserAsync(User);
            _context.AuditLogs.Add(new AuditLog
            {
                Action = $"{model.Role} account provisioned",
                EntityName = model.Role,
                UserId = adminUser?.Id,
                UserDisplayName = adminUser?.FullName,
                Status = "Success"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{model.Role} account created for {model.FullName}.";
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var adminUser = await _userManager.GetUserAsync(User);
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "User account updated",
                EntityName = "ApplicationUser",
                EntityId = user.Id,
                UserId = adminUser?.Id,
                UserDisplayName = adminUser?.FullName,
                Status = "Success"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{user.FullName} is now {(user.IsActive ? "active" : "deactivated")}.";
            return RedirectToAction(nameof(UserManagement));
        }

        // ---------------- Projects (system-wide view) ----------------

        public async Task<IActionResult> Projects()
        {
            ViewData["Title"] = "Projects";
            ViewData["ActiveNav"] = "projects";
            ViewData["DisplayName"] = await CurrentDisplayNameAsync();

            var projects = await _context.ReliefProjects
                .Include(p => p.CreatedByEmployee).ThenInclude(e => e!.User)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
            return View(projects);
        }

        // ---------------- Donations ----------------

        public async Task<IActionResult> Donations()
        {
            ViewData["Title"] = "Donations";
            ViewData["ActiveNav"] = "donations";
            ViewData["DisplayName"] = await CurrentDisplayNameAsync();

            var donations = await _context.Donations
                .Include(d => d.ReliefProject)
                .Include(d => d.Donor).ThenInclude(d => d!.User)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();
            return View(donations);
        }

        // ---------------- Volunteers ----------------

        public async Task<IActionResult> Volunteers()
        {
            ViewData["Title"] = "Volunteers";
            ViewData["ActiveNav"] = "volunteers";
            ViewData["DisplayName"] = await CurrentDisplayNameAsync();

            var volunteers = await _context.Volunteers.OrderByDescending(v => v.RegistrationDate).ToListAsync();
            return View(volunteers);
        }

        // ---------------- Settings ----------------

        public async Task<IActionResult> Settings()
        {
            ViewData["Title"] = "Settings";
            ViewData["ActiveNav"] = "settings";
            ViewData["DisplayName"] = await CurrentDisplayNameAsync();
            return View();
        }
    }
}
