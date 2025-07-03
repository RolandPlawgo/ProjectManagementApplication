using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.ProductIncrementViewModels;

namespace ProjectManagementApplication.Controllers
{
    public class ProductIncrementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductIncrementController(ApplicationDbContext context)
        {
            _context = context;
        }
        

        public async Task<IActionResult> Index(int id)
        {
            Project? project = await _context.Projects.Where(p => p.Id == id)
                .Include(p => p.Epics.OrderByDescending(e => e.Id)).ThenInclude(e => e.UserStories)
                .FirstOrDefaultAsync();
            if (project == null) return NotFound();

            List<SprintSummaryViewModel> sprintsVm = new List<SprintSummaryViewModel>();
            List<Sprint> sprints = await _context.Sprints.Where(s => s.ProjectId == id)
                .Include(s => s.UserStories).ThenInclude(u => u.Epic)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
            foreach (Sprint sprint in sprints)
            {
                List<UserStorySummaryViewModel> storiesVm = new List<UserStorySummaryViewModel>();
                foreach (UserStory story in sprint.UserStories)
                {
                    storiesVm.Add(new UserStorySummaryViewModel()
                    {
                        Id = story.Id,
                        Title = story.Title,
                        EpicTitle = story.Epic.Title
                    });
                }
                sprintsVm.Add(new SprintSummaryViewModel()
                {
                    Id = sprint.Id,
                    SprintGoal = sprint.SprintGoal,
                    UserStories = storiesVm
                });
            }

            List<EpicSummaryViewModel> epicsVm = new List<EpicSummaryViewModel>();
            foreach (Epic epic in project.Epics)
            {
                List<UserStorySummaryViewModel> storiesVm = new List<UserStorySummaryViewModel>();
                foreach (UserStory story in epic.UserStories)
                {
                    storiesVm.Add(new UserStorySummaryViewModel()
                    {
                        Id = story.Id,
                        Title = story.Title,
                        EpicTitle = story.Epic.Title
                    });
                }
                epicsVm.Add(new EpicSummaryViewModel()
                {
                    Id = epic.Id,
                    Title = epic.Title,
                    UserStories = storiesVm
                });
            }

            ProductIncrementViewModel model = new ProductIncrementViewModel()
            {
                ProjectId = id,
                ProjectName = project.Name,
                Sprints = sprintsVm,
                Epics = epicsVm
            };

            return View(model);
        }
    }
}
