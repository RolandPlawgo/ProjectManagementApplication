using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.UserStoryViewModels;
using System.Linq;
using System.Threading.Tasks;

public class UserStoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;

    public UserStoriesController(ApplicationDbContext context, IAuthorizationService authorizationService)
    {
        _context = context;
        _authorizationService = authorizationService;
    }

    // GET: /UserStories/Create?epicId=5
    [HttpGet]
    [Authorize(Roles = "Product Owner")]
    public async Task<IActionResult> Create(int epicId)
    {
        var epic = await _context.Epics.FirstOrDefaultAsync(e => e.Id == epicId);
        if (epic == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(epic.ProjectId));
        if (!authResult.Succeeded) return Forbid();

        var vm = new CreateUserStoryViewModel { EpicId = epicId };
        return PartialView("_CreateUserStory", vm);
    }

    // POST: /UserStories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Product Owner")]
    public async Task<IActionResult> Create(CreateUserStoryViewModel model)
    {
        if (!ModelState.IsValid)
            return PartialView("_CreateUserStory", model);

        var epic = await _context.Epics
            .Include(e => e.Project)
                .ThenInclude(p => p.Users)
            .FirstOrDefaultAsync(e => e.Id == model.EpicId);
        if (epic == null) return Json(new { success = false, error = "Epic not found." }); 

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(epic.ProjectId));
        if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

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

    // GET: /UserStories/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userStory = await _context.UserStories
            .Include(u => u.Epic)
            .Include(u => u.Subtasks)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (userStory == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(userStory.Epic.ProjectId));
        if (!authResult.Succeeded) return Forbid();

        var vm = new UserStoryDetailsViewModel
        {
            Id = userStory.Id,
            Title = userStory.Title,
            Description = userStory.Description,
            EpicTitle = userStory.Epic.Title
        };
        return PartialView("_UserStoryDetails", vm);
    }

    // GET: /UserStories/Edit/5
    [HttpGet]
    [Authorize(Roles = "Product Owner")]
    public async Task<IActionResult> Edit(int id)
    {
        var userStory = await _context.UserStories
            .Include(u => u.Epic)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (userStory == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(userStory.Epic.ProjectId));
        if (!authResult.Succeeded) return Forbid();

        var epics = await _context.Epics
            .Where(e => e.ProjectId == userStory.Epic.ProjectId)
            .ToListAsync();

        var vm = new EditUserStoryViewModel
        {
            Id = userStory.Id,
            EpicId = userStory.EpicId,
            Title = userStory.Title,
            Description = userStory.Description,
            Epics = epics.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Title,
                Selected = (e.Id == userStory.EpicId)
            }).ToList()
        };
        return PartialView("_EditUserStory", vm);
    }

    // POST: /UserStories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Product Owner")]
    public async Task<IActionResult> Edit(EditUserStoryViewModel vm)
    {
        if (!ModelState.IsValid)
            return PartialView("_EditUserStory", vm);

        var userStory = await _context.UserStories
            .Include(u => u.Epic)
            .FirstOrDefaultAsync(u => u.Id == vm.Id);
        if (userStory == null) return Json(new { success = false, error = "User story not found." });

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(userStory.Epic.ProjectId));
        if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

        userStory.EpicId = vm.EpicId;
        userStory.Title = vm.Title;
        userStory.Description = vm.Description;
        _context.Update(userStory);
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    // POST: /UserStories/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Product Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        var userStory = await _context.UserStories
            .Include(u => u.Epic)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (userStory == null) return Json(new { success = false, error = "User story not found." });

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(userStory.Epic.ProjectId));
        if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

        _context.UserStories.Remove(userStory);
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }
}
