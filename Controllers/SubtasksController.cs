using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Helpers;
using ProjectManagementApplication.Models.Subtasks;
using ProjectManagementApplication.Models.SubtasksViewModels;
using System.Threading.Tasks;

namespace ProjectManagementApplication.Controllers
{
    public class SubtasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public SubtasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: /Subtasks/Create?storyId=5
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int storyId)
        {
            var userStory = await _context.UserStories
                .Include(us => us.Sprint)
                .FirstOrDefaultAsync(us => us.Id == storyId);
            if (userStory == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(userStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            var vm = new CreateSubtaskViewModel { UserStoryId = storyId };
            return PartialView("_CreateSubtask", vm);
        }

        // POST: /Subtasks/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateSubtaskViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateSubtask", model);

            var userStory = await _context.UserStories
                .Include(us => us.Sprint)
                .FirstOrDefaultAsync(us => us.Id == model.UserStoryId);
            if (userStory == null) return Json(new { success = false, error = "User story not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(userStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            var subtask = new Subtask
            {
                Title = model.Title,
                Content = model.Content ?? "",
                UserStoryId = model.UserStoryId,
                Done = false
            };
            _context.Subtasks.Add(subtask);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // GET: /Subtasks/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var subtask = await _context.Subtasks
                .Include(s => s.UserStory)
                    .ThenInclude(us => us.Sprint)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (subtask == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(subtask.UserStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            var vm = new EditSubtaskViewModel
            {
                Id = subtask.Id,
                Title = subtask.Title,
                Content = subtask.Content,
                UserStoryId = subtask.UserStoryId
            };
            return PartialView("_EditSubtask", vm);
        }

        // POST: /Subtasks/Edit/5
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(EditSubtaskViewModel vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditSubtask", vm);

            var subtask = await _context.Subtasks
                .Include(s => s.UserStory)
                    .ThenInclude(us => us.Sprint)
                        .ThenInclude(sp => sp.Project)
                            .ThenInclude(p => p.Users)
                .FirstOrDefaultAsync(s => s.Id == vm.Id);
            if (subtask == null) return Json(new { success = false, error = "User story not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(subtask.UserStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            subtask.Title = vm.Title;
            subtask.Content = vm.Content ?? "";
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST: /Subtasks/Delete/5
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subtask = await _context.Subtasks
                .Include(s => s.UserStory)
                    .ThenInclude(us => us.Sprint)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (subtask == null) return Json(new { success = false, error = "User story not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(subtask.UserStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            _context.Subtasks.Remove(subtask);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // GET: /Subtasks/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var subtask = await _context.Subtasks
                .Include(s => s.UserStory)
                    .ThenInclude(us => us.Sprint)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (subtask == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(subtask.UserStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            var model = await BuildDetailsViewModel(id);
            return PartialView("_SubtaskDetails", model!);
        }

        // POST: /Subtasks/AddComment
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string content)
        {
            var subtask = await _context.Subtasks
                .Include(s => s.UserStory)
                    .ThenInclude(u => u.Sprint)
                .FirstOrDefaultAsync(s => s.Id == taskId);
            if (subtask == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(subtask.UserStory.Sprint.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            var user = await _userManager.GetUserAsync(User);
            var comment = new Comment
            {
                TaskId = taskId,
                Content = content,
                Author = user!,
                CreatedAt = DateTime.Now
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var model = await BuildDetailsViewModel(taskId);
            return PartialView("_SubtaskDetails", model!);
        }

        private async Task<SubtaskDetailsViewModel?> BuildDetailsViewModel(int subtaskId)
        {
            var subtask = await _context.Subtasks
                .Include(s => s.Comments)
                .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(s => s.Id == subtaskId);
            if (subtask == null) return null;

            var model = new SubtaskDetailsViewModel
            {
                Id = subtask.Id,
                Title = subtask.Title,
                Content = subtask.Content,
                Comments = subtask.Comments
                       .OrderBy(c => c.CreatedAt)
                    .Select(c => new CommentViewModel
                    {
                        Content = c.Content,
                        AuthorInitials = ApplicationUserHelper.UserInitials(c.Author),
                        CreatedAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                    }).ToList()
            };
            return model;
        }
    }
}
