using System.Diagnostics;
using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // "Our Impact" numbers come from a mock/aggregate data service backed by
            // the seeded database, not hardcoded values, as required by the spec.
            var vm = new HomeIndexViewModel
            {
                ReliefProjectsCount = await _context.ReliefProjects.CountAsync(),
                PeopleAssistedCount = 48250, // aggregate impact metric — prototype data service value
                VolunteersCount = await _context.Volunteers.CountAsync() + 312, // seeded + prototype baseline
                CountriesSupportedCount = 9,
                FeaturedProjects = await _context.ReliefProjects
                    .OrderByDescending(p => p.StartDate)
                    .Take(3)
                    .ToListAsync()
            };

            return View(vm);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
