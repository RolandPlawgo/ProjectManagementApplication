using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Models.SprintViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ProjectManagementApplication.Controllers
{
    public class SprintReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public SprintReviewController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index(int id)
        {
            Sprint? sprint = await _context.Sprints.Where(s => s.Id == id)
                .Include(s => s.Project)
                .FirstOrDefaultAsync();
            if (sprint == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            if (!sprint.Active || !sprint.EndDate.HasValue || sprint.EndDate > DateTime.Now)
            {
                return RedirectToAction("Index", "Sprint", new { id = sprint.ProjectId });
            }

            var productStories = await _context.UserStories.Where(u => u.SprintId == id && u.Status == Status.Backlog)
                .Include(u => u.Epic)
                .Include(u => u.Subtasks)
                .ToListAsync();
            var sprintStories = await _context.UserStories.Where(u => u.SprintId == id && u.Status == Status.Sprint)
                .Include(u => u.Epic)
                .Include(u => u.Subtasks)
                .ToListAsync();
            var productIncrementStories = await _context.UserStories.Where(u => u.SprintId == id && u.Status == Status.ProductIncrement)
                .Include(u => u.Epic)
                .Include(u => u.Subtasks)
                .ToListAsync();

            List<SprintReviewUserStorySummaryViewModel> backlogStoriesVm = new();
            foreach (var story in productStories)
            {
                backlogStoriesVm.Add(new SprintReviewUserStorySummaryViewModel()
                {
                    Id = story.Id,
                    Title = story.Title,
                    EpicTitle = story.Epic.Title,
                    AllTasksCount = story.Subtasks.Count(),
                    CompletedTasksCount = story.Subtasks.Where(s => s.Done).Count()
                });
            }
            List<SprintReviewUserStorySummaryViewModel> sprintStoriesVm = new();
            foreach (var story in sprintStories)
            {
                sprintStoriesVm.Add(new SprintReviewUserStorySummaryViewModel()
                {
                    Id = story.Id,
                    Title = story.Title,
                    EpicTitle = story.Epic.Title,
                    AllTasksCount = story.Subtasks.Count(),
                    CompletedTasksCount = story.Subtasks.Where(s => s.Done).Count()
                });
            }
            List<SprintReviewUserStorySummaryViewModel> productIncrementStoriesVm = new();
            foreach (var story in productIncrementStories)
            {
                productIncrementStoriesVm.Add(new SprintReviewUserStorySummaryViewModel()
                {
                    Id = story.Id,
                    Title = story.Title,
                    EpicTitle = story.Epic.Title,
                    AllTasksCount = story.Subtasks.Count(),
                    CompletedTasksCount = story.Subtasks.Where(s => s.Done).Count()
                });
            }

            var model = new SprintReviewViewModel()
            {
                SprintId = id,
                ProjectId = sprint.ProjectId,
                ProjectName = sprint.Project.Name,
                SprintGoal = sprint.SprintGoal,
                ProductBacklogUserStories = backlogStoriesVm,
                SprintBacklogUserStories = sprintStoriesVm,
                ProductIncrementUserStories = productIncrementStoriesVm
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveCard(int id, string targetList)
        {
            UserStory? userStory = await _context.UserStories
                .Where(u => u.Id == id)
                .Include(u => u.Sprint)
                .FirstOrDefaultAsync();
            if (userStory == null) return Json(new { success = false, error = "User story not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(userStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            if (targetList == "ProductBacklog")
            {
                userStory.Status = Status.Backlog;
            }
            if (targetList == "SprintBacklog")
            {
                userStory.Status = Status.Sprint;
            }
            if (targetList == "ProductIncrement")
            {
                userStory.Status = Status.ProductIncrement;
            }

            _context.UserStories.Update(userStory);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Product Owner")]
        public async Task<IActionResult> FinishSprint(int id)
        {
            Sprint? sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            sprint.Active = false;

            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "ProductIncrement", new {id = sprint.ProjectId});
        }
    }
}
