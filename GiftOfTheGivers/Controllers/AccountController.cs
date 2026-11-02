using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Controllers
{
    /// <summary>
    /// General login/registration for Donors and normal users.
    /// Public self-registration is intentionally Donor-only: there is no role
    /// selector anywhere on this controller or its views. Employee and Admin
    /// accounts are provisioned internally (see AdminDashboardController /
    /// seed data) and can never be created through this flow.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // Employees and Admins must use the general Donor-facing login only if they
            // are Donors; direct them to the correct experience otherwise is not required
            // here — the same credentials work, but Admins are encouraged to use /admin/login.
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToDashboard(user);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account has been temporarily locked due to multiple failed login attempts.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Phone,
                FullName = model.FullName,
                Role = UserRoles.Donor, // hard-coded — public registration can only ever create Donors
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, UserRoles.Donor);

            _context.Donors.Add(new Donor { UserId = user.Id });
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "New donor registration",
                EntityName = "Donor",
                UserId = user.Id,
                UserDisplayName = user.FullName,
                Status = "Success"
            });
            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["SuccessMessage"] = "Welcome to Gift of the Givers! Your donor account has been created.";
            return RedirectToAction("Index", "DonorDashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return RedirectToAction("AccessDenied", "Home");
        }

        private IActionResult RedirectToDashboard(ApplicationUser user)
        {
            return user.Role switch
            {
                UserRoles.Admin => RedirectToAction("Index", "AdminDashboard"),
                UserRoles.Employee => RedirectToAction("Index", "EmployeeDashboard"),
                _ => RedirectToAction("Index", "DonorDashboard")
            };
        }
    }
}
