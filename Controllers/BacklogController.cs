using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.BacklogViewModels;

namespace ProjectManagementApplication.Controllers
{
    public class BacklogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BacklogController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Epics)
                    .ThenInclude(e => e.UserStories.Where(us => us.Status == Status.Backlog))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var model = new BacklogViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Epics = project.Epics
                    .Select(e => new EpicSummaryViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        UserStories = e.UserStories
                            .Select(us => new UserStorySummaryViewModel
                            {
                                Id = us.Id,
                                Title = us.Title
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return View(model);
        }

        // GET: /Backlog/CreateEpic?projectId=5
        [HttpGet]
        public IActionResult CreateEpic(int projectId)
        {
            var vm = new EpicViewModel { ProjectId = projectId };
            return PartialView("_CreateEpic", vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEpic(EpicViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEpic", model);

            var epic = new Epic
            {
                Title = model.Title,
                ProjectId = model.ProjectId
            };
            _context.Epics.Add(epic);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: /Backlog/EditEpic?id=5
        [HttpGet]
        public async Task<IActionResult> EditEpic(int id)
        {
            var epic = await _context.Epics.FindAsync(id);
            if (epic == null) return NotFound();

            var vm = new EpicViewModel
            {
                Id = epic.Id,
                ProjectId = epic.ProjectId,
                Title = epic.Title
            };
            return PartialView("_EditEpic", vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditEpic(EpicViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditEpic", model);

            var epic = await _context.Epics.FindAsync(model.Id);
            if (epic == null) return NotFound();

            epic.Title = model.Title;
            _context.Epics.Update(epic);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEpic(int id)
        {
            var epic = await _context.Epics
                .Include(e => e.UserStories)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (epic == null) return Json(new { success = false });

            _context.UserStories.RemoveRange(epic.UserStories);
            _context.Epics.Remove(epic);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }





        // GET: /Backlog/CreateEpic?epicId=5
        [HttpGet]
        public IActionResult CreateStory(int epicId)
        {
            var vm = new CreateUserStoryViewModel { EpicId = epicId };
            return PartialView("_CreateUserStory", vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStory(CreateUserStoryViewModel model)
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
        public async Task<IActionResult> EditStory(int id)
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
        
        [HttpPost]
        public async Task<IActionResult> EditStory(EditUserStoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditUserStory", model);
            }

            try
            {
                var story = new UserStory
                {
                    Id = model.Id,
                    EpicId = model.EpicId,
                    Title = model.Title,
                    Description = model.Description
                };

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
        public async Task<IActionResult> DeleteStory(int id)
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
