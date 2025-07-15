
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.SprintViewModels;

namespace ProjectManagementApplication.Controllers
{
    public class ScrumBoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        public ScrumBoardController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
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

            if (!sprint.Active || (sprint.EndDate.HasValue && sprint.EndDate < DateTime.Now))
            {
                return RedirectToAction("Index", "Sprint", new { id = sprint.ProjectId });
            }

            List<UserStory> userStories = await _context.UserStories
                .Where(u => u.SprintId == id && u.Status == Status.Sprint)
                .Include(u => u.Subtasks).ThenInclude(s => s.Comments)
                .Include(u => u.Subtasks).ThenInclude(s => s.AssignedUser)
                .Include(u => u.Epic)
                .ToListAsync();
            List<UserStorySummaryViewModel> userStoriesVm = new List<UserStorySummaryViewModel>();
            List<ScrumBoardSubtaskSummaryViewModel> toDoTasks = new List<ScrumBoardSubtaskSummaryViewModel>();
            List<ScrumBoardSubtaskSummaryViewModel> inProgressTasks = new List<ScrumBoardSubtaskSummaryViewModel>();
            List<ScrumBoardSubtaskSummaryViewModel> DoneTasks = new List<ScrumBoardSubtaskSummaryViewModel>();
            foreach (UserStory userStory in userStories)
            {
                userStoriesVm.Add(new UserStorySummaryViewModel()
                {
                    Id = userStory.Id,
                    Title = userStory.Title,
                    EpicTitle = userStory.Epic.Title
                });

                foreach (Subtask subtask in userStory.Subtasks)
                {
                    if (subtask.AssignedUser == null && !subtask.Done)
                    {
                        toDoTasks.Add(new ScrumBoardSubtaskSummaryViewModel()
                        {
                            Id = subtask.Id,
                            UserStoryId = userStory.Id,
                            Title = subtask.Title,
                            CommentsCount = subtask.Comments.Count()
                        });
                    }
                    if (subtask.AssignedUser != null && !subtask.Done)
                    {
                        inProgressTasks.Add(new ScrumBoardSubtaskSummaryViewModel()
                        {
                            Id = subtask.Id,
                            UserStoryId = userStory.Id,
                            Title = subtask.Title,
                            CommentsCount = subtask.Comments.Count(),
                            AssignedUserInitials = Helpers.ApplicationUserHelper.UserInitials(subtask.AssignedUser)
                        });
                    }
                    if (subtask.AssignedUser != null && subtask.Done)
                    {
                        DoneTasks.Add(new ScrumBoardSubtaskSummaryViewModel()
                        {
                            Id = subtask.Id,
                            UserStoryId = userStory.Id,
                            Title = subtask.Title,
                            CommentsCount = subtask.Comments.Count(),
                            AssignedUserInitials = Helpers.ApplicationUserHelper.UserInitials(subtask.AssignedUser)
                        });
                    }
                }
            }

            ScrumBoardViewModel model = new ScrumBoardViewModel()
            {
                ProjectId = sprint.ProjectId,
                ProjectName = sprint.Project.Name,
                SprintId = sprint.Id,
                SprintGoal = sprint.SprintGoal,
                DaysToEndOfSprint = (sprint.EndDate - DateTime.Now)?.Days ?? 0,
                UserStories = userStoriesVm,
                ToDoTasks = toDoTasks,
                InProgressTasks = inProgressTasks,
                DoneTasks = DoneTasks
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MoveCard (int id, string targetList)
        {
            Subtask? task = await _context.Subtasks
                .Where(s => s.Id == id)
                .Include(t => t.UserStory)
                    .ThenInclude(u => u.Sprint)
                .FirstOrDefaultAsync();
            if (task == null)
            {
                return Json(new { success = false });
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(task.UserStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            if (targetList == "todo")
            {
                task.AssignedUserId = null;
                task.AssignedUser = null;
                task.Done = false;
            }
            if (targetList == "inprogress")
            {
                var user = await _userManager.GetUserAsync(User);
                task.AssignedUserId = user?.Id;
                task.AssignedUser = user;
                task.Done = false;
            }
            if (targetList == "done")
            {
                var user = await _userManager.GetUserAsync(User);
                task.AssignedUserId = user?.Id;
                task.AssignedUser = user;
                task.Done = true;
            }
            _context.Subtasks.Update(task);
            await _context.SaveChangesAsync();

            string initials = "";
            if (task.AssignedUser != null)
                initials = Helpers.ApplicationUserHelper.UserInitials(task.AssignedUser);

            return Json(new { success = true, initials });
        }
    }
}
