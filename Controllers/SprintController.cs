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
    public class SprintController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public SprintController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index(int id)
        {
            Project? project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(project.Id));
            if (!authResult.Succeeded) return Forbid();


            Sprint? activeSprint = await _context.Sprints.Where(s => s.ProjectId == id && s.Active == true && s.EndDate.HasValue && s.EndDate > DateTime.Now).FirstOrDefaultAsync();
            if (activeSprint != null)
            {
                return RedirectToAction("Index", "ScrumBoard", new { id = activeSprint.Id });
            }

            Sprint? completedSprint = await _context.Sprints.Where(s => s.ProjectId == id && s.Active == true && s.EndDate.HasValue && s.EndDate <= DateTime.Now).FirstOrDefaultAsync();
            if (completedSprint != null)
            {
                return RedirectToAction("Index", "SprintReview", new { id = completedSprint.Id });
            }

            Sprint? sprint = await _context.Sprints.Where(s => s.ProjectId == id && s.Active == false && s.EndDate == null).FirstOrDefaultAsync();
            if (sprint == null)
            {
                Sprint newSprint = new Sprint()
                {
                    Project = project,
                    Active = false,
                    SprintGoal = ""
                };
                var createdSprint = await _context.Sprints.AddAsync(newSprint);
                await _context.SaveChangesAsync();
                sprint = createdSprint.Entity;
            }

            return RedirectToAction("Index", "SprintPlanning", new { id = sprint.Id} );
        }
    }
}
