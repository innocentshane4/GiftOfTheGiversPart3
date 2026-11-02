using GiftOfTheGivers.Data;
using GiftOfTheGivers.Models;
using GiftOfTheGivers.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, ProjectStatus? status, ProjectCategory? category)
        {
            var query = _context.ReliefProjects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p => p.Name.Contains(term) || p.Location.Contains(term) || p.Description.Contains(term));
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (category.HasValue)
            {
                query = query.Where(p => p.Category == category.Value);
            }

            var vm = new ProjectListViewModel
            {
                SearchTerm = search,
                StatusFilter = status,
                CategoryFilter = category,
                Projects = await query.OrderByDescending(p => p.StartDate).ToListAsync()
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.ReliefProjects
                .Include(p => p.Updates.OrderByDescending(u => u.PostedDate))
                    .ThenInclude(u => u.Employee)
                        .ThenInclude(e => e!.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }
    }
}
