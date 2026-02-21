using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Controllers
{
    public class SprintController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISprintService _sprintService;

        public SprintController(UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService, ISprintService sprintService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _sprintService = sprintService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Forbid();


            int? activeSprintId = await _sprintService.GetActiveSprintId(id);
            if (activeSprintId != null)
            {
                return RedirectToAction("Index", "ScrumBoard", new { id = activeSprintId });
            }

            int? completedSprintd = await _sprintService.GetCompletedSprintId(id);
            if (completedSprintd != null)
            {
                return RedirectToAction("Index", "SprintReview", new { id = completedSprintd });
            }

            int? sprintId = await _sprintService.GetOrCreateNewSprintAsync(id);
            if (sprintId == null) return NotFound();

            return RedirectToAction("Index", "SprintPlanning", new { id = (int)sprintId} );
        }
    }
}
