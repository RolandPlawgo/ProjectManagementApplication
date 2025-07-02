using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Models.SprintViewModels;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApplication.Controllers
{
    public class SprintReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SprintReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int id)
        {
            Sprint? sprint = await _context.Sprints.Where(s => s.Id == id)
                .Include(s => s.Project)
                .FirstOrDefaultAsync();
            if (sprint == null) return NotFound();

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
            UserStory? story = await _context.UserStories.FindAsync(id);
            if (story == null) return NotFound();

            if (targetList == "ProductBacklog")
            {
                story.Status = Status.Backlog;
            }
            if (targetList == "SprintBacklog")
            {
                story.Status = Status.Sprint;
            }
            if (targetList == "ProductIncrement")
            {
                story.Status = Status.ProductIncrement;
            }

            _context.UserStories.Update(story);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishSprint(int id)
        {
            Sprint? sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return NotFound();

            sprint.Active = false;

            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "ProductIncrement");
        }
    }
}
