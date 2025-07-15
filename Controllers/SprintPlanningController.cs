using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.SprintViewModels;
using System.Threading.Tasks;
using System.Linq;

namespace ProjectManagementApplication.Controllers
{
    public class SprintPlanningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public SprintPlanningController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var sprint = await _context.Sprints
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.Id == id);
            if (sprint == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(sprint.ProjectId));
            if (!authResult.Succeeded)
                return Forbid();

            if (sprint.Active)
            {
                return RedirectToAction("Index", "Sprint", new { id = sprint.ProjectId });
            }

            var backlogStories = await _context.UserStories
                .Where(u => u.Status == Status.Backlog)
                .Include(u => u.Epic).Include(u => u.Subtasks)
                .ToListAsync();
            var sprintStories = await _context.UserStories
                .Where(u => u.Status == Status.Sprint && u.SprintId == sprint.Id)
                .Include(u => u.Epic).Include(u => u.Subtasks)
                .ToListAsync();

            var model = new SprintPlanningViewModel
            {
                ProjectId = sprint.ProjectId,
                ProjectName = sprint.Project.Name,
                SprintId = sprint.Id,
                SprintGoal = sprint.SprintGoal,
                BacklogUserStories = backlogStories
                                          .Select(us => new UserStorySummaryViewModel
                                          {
                                              Id = us.Id,
                                              Title = us.Title,
                                              EpicTitle = us.Epic.Title
                                          }).ToList(),
                SprintUserStories = sprintStories
                                          .Select(us => new UserStorySummaryViewModel
                                          {
                                              Id = us.Id,
                                              Title = us.Title,
                                              EpicTitle = us.Epic.Title
                                          }).ToList(),
                Subtasks = backlogStories.Concat(sprintStories)
                                          .SelectMany(us => us.Subtasks
                                            .Select(st => new SubtaskSummaryViewModel
                                            {
                                                Id = st.Id,
                                                UserStoryId = us.Id,
                                                Title = st.Title
                                            }))
                                          .ToList()
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> MoveUserStory(int id, int sprintId, string targetList)
        {
            var userStory = await _context.UserStories.Where(u => u.Id == id)
                .Include(u => u.Epic)
                .FirstOrDefaultAsync();
            if (userStory == null) return Json(new { success = false, error = "User story not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(userStory.Epic.ProjectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            if (targetList == "Sprint" && userStory.Status == Status.Backlog)
            {
                userStory.Status = Status.Sprint;
                userStory.SprintId = sprintId;
            }
            else if (targetList == "Backlog" && userStory.Status == Status.Sprint)
            {
                userStory.Status = Status.Backlog;
                userStory.SprintId = null;
            }

            _context.Update(userStory);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddSubtask(int storyId, int sprintId, string title)
        {
            var userStory = await _context.UserStories.FirstOrDefaultAsync(u => u.Id == storyId);
            if (userStory == null) return NotFound();

            var sprint = await _context.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId);
            if (sprint == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            _context.Subtasks.Add(new Subtask
            {
                UserStoryId = storyId,
                Title = title,
                Content = ""
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = sprintId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSprintGoal(int sprintId, string sprintGoal)
        {
            var sprint = await _context.Sprints
                .FirstOrDefaultAsync(s => s.Id == sprintId);
            if (sprint == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            sprint.SprintGoal = sprintGoal;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = sprintId });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> StartSprint(int id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sprint == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            sprint.Active = true;
            sprint.EndDate = DateTime.Now.AddDays(sprint.Project.SprintDuration * 7);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Sprint", new { id = sprint.ProjectId });
        }
    }
}
