using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.BacklogViewModels;
using ProjectManagementApplication.Models.UserStoryViewModels;

namespace ProjectManagementApplication.Controllers
{
    public class BacklogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BacklogController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Epics)
                    .ThenInclude(e => e.UserStories.Where(us => us.Status == Status.Backlog).OrderByDescending(us => us.Id))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var model = new BacklogViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Epics = project.Epics
                    .Select(e => new EpicSummaryViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        UserStories = e.UserStories
                            .Select(us => new UserStorySummaryViewModel
                            {
                                Id = us.Id,
                                Title = us.Title
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return View(model);
        }
    }
}
