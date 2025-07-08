using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.SprintViewModels;
using ProjectManagementApplication.Models.UserStoryViewModels;

namespace ProjectManagementApplication.Controllers
{
    [Authorize(Roles = "Product Owner")]
    public class UserStoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserStoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /UserStory/CreateStory?epicId=5
        [HttpGet]
        public IActionResult Create(int epicId)
        {
            var vm = new CreateUserStoryViewModel { EpicId = epicId };
            return PartialView("_CreateUserStory", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserStoryViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateUserStory", model);

            try
            {
                var story = new UserStory
                {
                    EpicId = model.EpicId,
                    Title = model.Title,
                    Description = model.Description,
                    Status = Status.Backlog
                };

                _context.UserStories.Add(story);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                // log ex here
                ModelState.AddModelError("", "Error saving user story.");
                return PartialView("_CreateUserStory", model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userStory = await _context.UserStories.FindAsync(id);
            if (userStory == null) return NotFound();

            Epic? epic = await _context.Epics.FindAsync(userStory.EpicId);
            if (epic == null)
            {
                return null;
            }
            int? projectId = epic.ProjectId;
            if (projectId == null)
            {
                return NotFound();
            }
            List<Epic> epics = await _context.Epics.Where(epic => epic.ProjectId == projectId).ToListAsync();

            var model = new EditUserStoryViewModel
            {
                Id = userStory.Id,
                EpicId = userStory.EpicId,
                Title = userStory.Title,
                Description = userStory.Description,
                Epics = epics.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title,
                    Selected = e.Id == userStory.EpicId
                }).ToList()
            };

            return PartialView("_EditUserStory", model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var story = await _context.UserStories
                .Include(u => u.Epic)
                .Include(u => u.Subtasks)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (story == null) return NotFound();

            var model = new UserStoryDetailsViewModel
            {
                Id = story.Id,
                Title = story.Title,
                Description = story.Description,
                EpicTitle = story.Epic?.Title ?? ""
            };

            return PartialView("_UserStoryDetails", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserStoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditUserStory", model);
            }

            try
            {
                UserStory? story = await _context.UserStories.FindAsync(model.Id);
                if (story == null)
                    return NotFound();

                story.EpicId = model.EpicId;
                story.Title = model.Title;
                story.Description = model.Description;

                _context.UserStories.Update(story);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes.");
                return PartialView("_EditUserStory", model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                UserStory? userStory = await _context.UserStories.FindAsync(id);
                if (userStory == null)
                {
                    return NotFound();
                }
                _context.UserStories.Remove(userStory);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
