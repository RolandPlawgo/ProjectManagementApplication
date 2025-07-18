using Microsoft.AspNetCore.Authorization;
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
        private readonly IAuthorizationService _authorizationService;

        public ProductIncrementController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }
        

        public async Task<IActionResult> Index(int id)
        {
            Project? project = await _context.Projects.Where(p => p.Id == id)
                .Include(p => p.Epics.OrderByDescending(e => e.Id)).ThenInclude(e => e.UserStories)
                .FirstOrDefaultAsync();
            if (project == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Forbid();

            List<SprintSummaryViewModel> sprintsVm = new List<SprintSummaryViewModel>();
            List<Sprint> sprints = await _context.Sprints.Where(s => s.ProjectId == id && s.Active == false && s.EndDate < DateTime.Now)
                .Include(s => s.UserStories.Where(u => u.Status == Status.ProductIncrement))
                    .ThenInclude(u => u.Epic)
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
