using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    [Authorize(Policy = "EmployeeOnly")]
    public class EmployeeDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _context.Employees.Include(e => e.User).FirstOrDefaultAsync(e => e.UserId == user.Id);
        }

        public async Task<IActionResult> Index()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return NotFound();

            // SQLite's EF Core provider cannot translate SUM() over a decimal column into SQL,
            // so the amounts are pulled back first and summed in memory.
            var donationAmounts = await _context.Donations.Select(d => d.Amount).ToListAsync();

            var vm = new EmployeeDashboardViewModel
            {
                EmployeeName = employee.User?.FullName ?? "Employee",
                ActiveProjectsCount = await _context.ReliefProjects.CountAsync(p => p.Status == ProjectStatus.InProgress),
                RegisteredVolunteersCount = await _context.Volunteers.CountAsync(),
                TotalDonationValue = donationAmounts.Sum(),
                RecentUpdates = await _context.ProjectUpdates
                    .Include(u => u.ReliefProject)
                    .Include(u => u.Employee).ThenInclude(e => e!.User)
                    .OrderByDescending(u => u.PostedDate)
                    .Take(5)
                    .ToListAsync()
            };

            ViewData["Title"] = "Dashboard";
            ViewData["ActiveNav"] = "dashboard";
            ViewData["DisplayName"] = vm.EmployeeName;
            return View(vm);
        }

        // ---------------- Projects ----------------

        public async Task<IActionResult> Projects()
        {
            ViewData["Title"] = "Projects";
            ViewData["ActiveNav"] = "projects";
            ViewData["DisplayName"] = (await GetCurrentEmployeeAsync())?.User?.FullName;
            var projects = await _context.ReliefProjects.OrderByDescending(p => p.StartDate).ToListAsync();
            return View(projects);
        }

        [HttpGet]
        public IActionResult CreateProject()
        {
            ViewData["Title"] = "Create Project";
            ViewData["ActiveNav"] = "projects";
            return View(new ProjectFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(ProjectFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Create Project";
                ViewData["ActiveNav"] = "projects";
                return View(model);
            }

            var employee = await GetCurrentEmployeeAsync();

            var project = new ReliefProject
            {
                Name = model.Name,
                Description = model.Description,
                Location = model.Location,
                Category = model.Category,
                StartDate = model.StartDate,
                ExpectedCompletionDate = model.ExpectedCompletionDate,
                Budget = model.Budget,
                Status = model.Status,
                ProgressPercentage = model.ProgressPercentage,
                CreatedByEmployeeId = employee?.Id
            };

            _context.ReliefProjects.Add(project);
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "Project created",
                EntityName = "ReliefProject",
                UserId = employee?.UserId,
                UserDisplayName = employee?.User?.FullName,
                Status = "Success"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Project \"{project.Name}\" was created.";
            return RedirectToAction(nameof(Projects));
        }

        [HttpGet]
        public async Task<IActionResult> EditProject(int id)
        {
            var project = await _context.ReliefProjects.FindAsync(id);
            if (project == null) return NotFound();

            ViewData["Title"] = "Edit Project";
            ViewData["ActiveNav"] = "projects";

            var model = new ProjectFormViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Location = project.Location,
                Category = project.Category,
                StartDate = project.StartDate,
                ExpectedCompletionDate = project.ExpectedCompletionDate,
                Budget = project.Budget,
                Status = project.Status,
                ProgressPercentage = project.ProgressPercentage
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(ProjectFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Edit Project";
                ViewData["ActiveNav"] = "projects";
                return View(model);
            }

            var project = await _context.ReliefProjects.FindAsync(model.Id);
            if (project == null) return NotFound();

            project.Name = model.Name;
            project.Description = model.Description;
            project.Location = model.Location;
            project.Category = model.Category;
            project.StartDate = model.StartDate;
            project.ExpectedCompletionDate = model.ExpectedCompletionDate;
            project.Budget = model.Budget;
            project.Status = model.Status;
            project.ProgressPercentage = model.ProgressPercentage;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Project \"{project.Name}\" was updated.";
            return RedirectToAction(nameof(Projects));
        }

        // ---------------- Project Updates ----------------

        public async Task<IActionResult> ProjectUpdates()
        {
            ViewData["Title"] = "Project Updates";
            ViewData["ActiveNav"] = "updates";
            ViewData["DisplayName"] = (await GetCurrentEmployeeAsync())?.User?.FullName;

            var updates = await _context.ProjectUpdates
                .Include(u => u.ReliefProject)
                .Include(u => u.Employee).ThenInclude(e => e!.User)
                .OrderByDescending(u => u.PostedDate)
                .ToListAsync();

            return View(updates);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProjectUpdate()
        {
            ViewData["Title"] = "Post Project Update";
            ViewData["ActiveNav"] = "updates";

            var model = new ProjectUpdateFormViewModel
            {
                AvailableProjects = await _context.ReliefProjects.OrderBy(p => p.Name).ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProjectUpdate(ProjectUpdateFormViewModel model)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.AvailableProjects = await _context.ReliefProjects.OrderBy(p => p.Name).ToListAsync();
                ViewData["Title"] = "Post Project Update";
                ViewData["ActiveNav"] = "updates";
                return View(model);
            }

            var update = new ProjectUpdate
            {
                Title = model.Title,
                Description = model.Description,
                ReliefProjectId = model.ReliefProjectId,
                EmployeeId = employee.Id,
                PostedDate = DateTime.UtcNow
            };

            _context.ProjectUpdates.Add(update);
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "Project update posted",
                EntityName = "ProjectUpdate",
                UserId = employee.UserId,
                UserDisplayName = employee.User?.FullName,
                Status = "Success"
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Project update posted.";
            return RedirectToAction(nameof(ProjectUpdates));
        }

        // ---------------- Volunteers ----------------

        public async Task<IActionResult> Volunteers()
        {
            ViewData["Title"] = "Volunteers";
            ViewData["ActiveNav"] = "volunteers";
            ViewData["DisplayName"] = (await GetCurrentEmployeeAsync())?.User?.FullName;

            var volunteers = await _context.Volunteers.OrderByDescending(v => v.RegistrationDate).ToListAsync();
            return View(volunteers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVolunteerStatus(int id, VolunteerStatus status)
        {
            var volunteer = await _context.Volunteers.FindAsync(id);
            if (volunteer == null) return NotFound();

            volunteer.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{volunteer.FullName}'s registration was marked as {status}.";
            return RedirectToAction(nameof(Volunteers));
        }

        // ---------------- Donations / Certificates (read-only for employees) ----------------

        public async Task<IActionResult> Donations()
        {
            ViewData["Title"] = "Donations";
            ViewData["ActiveNav"] = "donations";
            ViewData["DisplayName"] = (await GetCurrentEmployeeAsync())?.User?.FullName;

            var donations = await _context.Donations
                .Include(d => d.ReliefProject)
                .Include(d => d.Donor).ThenInclude(d => d!.User)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return View(donations);
        }

        public async Task<IActionResult> TaxCertificates()
        {
            ViewData["Title"] = "Tax Certificates";
            ViewData["ActiveNav"] = "certificates";
            ViewData["DisplayName"] = (await GetCurrentEmployeeAsync())?.User?.FullName;

            var certificates = await _context.TaxCertificates
                .Include(t => t.Donation).ThenInclude(d => d!.Donor).ThenInclude(d => d!.User)
                .OrderByDescending(t => t.IssuedDate)
                .ToListAsync();

            return View(certificates);
        }

        public async Task<IActionResult> Reports()
        {
            ViewData["Title"] = "Reports";
            ViewData["ActiveNav"] = "reports";
            ViewData["DisplayName"] = (await GetCurrentEmployeeAsync())?.User?.FullName;

            // SQLite's EF Core provider cannot translate SUM() over a decimal column into SQL,
            // so both aggregates below are grouped/summed in memory after materializing the rows.
            var allProjects = await _context.ReliefProjects.Select(p => new { p.Category, p.Budget }).ToListAsync();
            var byCategory = allProjects
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Count = g.Count(), Budget = g.Sum(p => p.Budget) })
                .ToList();

            var donationAmounts = await _context.Donations.Select(d => d.Amount).ToListAsync();

            ViewData["ByCategory"] = byCategory.ToDictionary(x => x.Category.ToString(), x => (x.Count, x.Budget));
            ViewData["TotalDonations"] = donationAmounts.Sum();
            ViewData["TotalVolunteers"] = await _context.Volunteers.CountAsync();

            return View();
        }
    }
}
