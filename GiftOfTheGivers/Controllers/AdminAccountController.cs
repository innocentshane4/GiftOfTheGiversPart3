using GiftOfTheGivers.Models;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Controllers
{
    /// <summary>
    /// A genuinely separate login route for Administrators, reachable at /admin/login.
    /// This is intentionally its own controller/view rather than a toggle inside the
    /// general login form. Admin accounts cannot be created here or anywhere public —
    /// they are provisioned internally (see DbInitializer / User Management).
    /// </summary>
    public class AdminAccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminAccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.Role != UserRoles.Admin || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid administrator credentials.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account has been temporarily locked due to multiple failed login attempts.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid administrator credentials.");
            }

            return View(model);
        }
    }
}
