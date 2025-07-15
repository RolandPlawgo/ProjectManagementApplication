using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.BacklogViewModels;

public class BacklogController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthorizationService _authorizationService;

    public BacklogController(
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
        var project = await _context.Projects
            .Include(p => p.Users)
            .Include(p => p.Epics)
                .ThenInclude(e => e.UserStories
                    .Where(u => u.Status == Status.Backlog)
                    .OrderByDescending(u => u.Id))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
        if (!authResult.Succeeded) return Forbid();

        var model = new BacklogViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Epics = project.Epics.Select(e => new EpicSummaryViewModel
            {
                Id = e.Id,
                Title = e.Title,
                UserStories = e.UserStories.Select(us => new UserStorySummaryViewModel
                {
                    Id = us.Id,
                    Title = us.Title
                }).ToList()
            }).ToList()
        };

        return View(model);
    }
}
