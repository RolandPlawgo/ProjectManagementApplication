using Humanizer;
using Microsoft.AspNetCore.Identity;
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

        public SubtasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Subtasks/Create?storyId=5
        [HttpGet("Create")]
        public IActionResult Create(int storyId)
        {
            var model = new CreateSubtaskViewModel { UserStoryId = storyId };
            return PartialView("_CreateSubtask", model);
        }

        // POST: /Subtasks/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateSubtaskViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateSubtask", model);

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
            var subtask = await _context.Subtasks.FindAsync(id);
            if (subtask == null) return NotFound();

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

            var subtask = await _context.Subtasks.FindAsync(vm.Id);
            if (subtask == null) return NotFound();

            subtask.Title = vm.Title;
            subtask.Content = vm.Content ?? "";

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // POST: /Subtasks/Delete/5
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subtask = await _context.Subtasks.FindAsync(id);
            if (subtask == null) return NotFound();

            _context.Subtasks.Remove(subtask);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await BuildDetailsViewModel(id);
            if (model == null) return NotFound();
            return PartialView("_SubtaskDetails", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) Unauthorized();
            var comment = new Comment
            {
                TaskId = taskId,
                Content = content,
                Author = user!,
                CreatedAt = DateTime.Now,
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var model = await BuildDetailsViewModel(taskId);
            if (model == null) return NotFound();
            return PartialView("_SubtaskDetails", model);
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
