using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.SprintViewModels;

namespace ProjectManagementApplication.Controllers
{
    public class SprintPlanningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SprintPlanningController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int id)
        {
            Sprint? sprint = await _context.Sprints.Where(s => s.Id == id)
                .Include(s => s.Project)
                .FirstOrDefaultAsync();
            if (sprint == null || sprint.UserStories == null)
                return NotFound();

            List<UserStory> backlogUserStories = await _context.UserStories.Where(u => u.Status == Status.Backlog)
                .Include(u => u.Epic)
                .Include(u => u.Subtasks)
                .ToListAsync();
            List<UserStory> sprintUserStories = await _context.UserStories.Where(u => u.Status == Status.Sprint && u.SprintId == sprint.Id)
                .Include(u => u.Subtasks).Include(u => u.Epic).ToListAsync();

            List<UserStorySummaryViewModel> backlogUserStoriesVm = new List<UserStorySummaryViewModel>();
            foreach (UserStory userStory in backlogUserStories)
            {
                backlogUserStoriesVm.Add(new UserStorySummaryViewModel()
                {
                    Id = userStory.Id,
                    Title = userStory.Title,
                    EpicTitle = userStory.Epic.Title
                });
            }
            List<UserStorySummaryViewModel> sprintUserStoriesVm = new List<UserStorySummaryViewModel>();
            foreach (UserStory userStory in sprintUserStories)
            {
                sprintUserStoriesVm.Add(new UserStorySummaryViewModel()
                {
                    Id = userStory.Id,
                    Title = userStory.Title,
                    EpicTitle = userStory.Epic.Title
                });
            }
            List<UserStory> allUserStories = new List<UserStory>();
            allUserStories.AddRange(backlogUserStories);
            allUserStories.AddRange(sprintUserStories);
            List<SubtaskSummaryViewModel> subtasksVm = new List<SubtaskSummaryViewModel>();
            foreach (UserStory userStory in allUserStories)
            {
                foreach (Subtask subtask in userStory.Subtasks)
                {
                    subtasksVm.Add(new SubtaskSummaryViewModel()
                    {
                        Id = subtask.Id,
                        UserStoryId = userStory.Id,
                        Title = subtask.Title
                    });
                }
            }

            SprintPlanningViewModel model = new SprintPlanningViewModel()
            {
                ProjectId = sprint.ProjectId,
                ProjectName = sprint.Project.Name,
                SprintId = sprint.Id,
                SprintGoal = sprint.SprintGoal,
                BacklogUserStories = backlogUserStoriesVm,
                SprintUserStories = sprintUserStoriesVm,
                Subtasks = subtasksVm,
            };
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MoveUserStory(int id, int sprintId, string targetList)
        {
            UserStory? userStory = await _context.UserStories.FindAsync(id);
            if (userStory == null)
            {
                return Json(new { success = false });
            }
            if (targetList == "Sprint")
            {
                if(userStory.Status == Status.Backlog)
                {
                    userStory.Status = Status.Sprint;
                    userStory.SprintId = sprintId;
                }
            }
            if (targetList == "Backlog")
            {
                if (userStory.Status == Status.Sprint)
                {
                    userStory.Status = Status.Backlog;
                    userStory.SprintId = null;
                }
            }
            _context.UserStories.Update(userStory);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddSubtask(int storyId, int sprintId, string title)
        {
            await _context.Subtasks.AddAsync(new Subtask()
            {
                UserStoryId = storyId,
                Title = title,
                Content = ""
            });
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { id = sprintId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSprintGoal(int sprintId, string sprintGoal)
        {
            var sprint = await _context.Sprints.FindAsync(sprintId);
            if (sprint == null) return NotFound();

            sprint.SprintGoal = sprintGoal;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = sprintId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> StartSprint(int id)
        {
            Sprint? sprint = await _context.Sprints.Where(s => s.Id == id)
                .Include(s => s.Project)
                .FirstOrDefaultAsync();
            if (sprint == null) return NotFound();

            sprint.Active = true;
            sprint.EndDate = DateTime.Now.AddDays(sprint.Project.SprintDuration * 7);

            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Sprint", new { id = sprint.ProjectId });
        }
    }
}
