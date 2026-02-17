using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Services.Interfaces;
using ProjectManagementApplication.Dto.Requests.SprintPlanningRequests;

namespace ProjectManagementApplication.Controllers
{
    public class SprintPlanningController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISprintPlanningService _sprintPlanningService;
        private readonly ISprintService _sprintService;

        public SprintPlanningController(
            UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService,
            ISprintPlanningService sprintPlanningService,
            ISprintService sprintService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _sprintPlanningService = sprintPlanningService;
            _sprintService = sprintService;
        }

        public async Task<IActionResult> Index(int id)
        {
            int? projectId = await _sprintService.GetProjectIdForSprintAsync(id);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded)
                return Forbid();

            bool? isSprintActive = await _sprintService.IsSprintActiveAsync(id);
            if (isSprintActive == null) return NotFound();
            if ((bool)isSprintActive)
            {
                return RedirectToAction("Index", "Sprint", new { id = (int)projectId });
            }

            var model = await _sprintPlanningService.GetSprintPlanningAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<JsonResult> MoveUserStory(int id, int sprintId, string targetList)
        {
            int? projectId = await _sprintService.GetProjectIdForSprintAsync(sprintId);
            if (projectId == null) return Json(new { success = false });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });


            Status? targetStatus = null;
            if (targetList == "Sprint") targetStatus = Status.Sprint;
            else if (targetList == "Backlog") targetStatus = Status.Backlog;
            if (targetStatus == null) return Json(new { success = false });

            bool success = await _sprintPlanningService.MoveUserStory(new MoveUserStoryRequest
            {
                UserStoryId = id,
                TargetStatus = (Status)targetStatus,
                SprintId = sprintId
            });
            if (!success) return Json(new { success = false });

            return Json(new { success = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetSprintGoal(int sprintId, string sprintGoal)
        {
            int? projectId = await _sprintService.GetProjectIdForSprintAsync(sprintId);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            var success = await _sprintPlanningService.SetSprintGoalAsync(sprintId, sprintGoal);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Index), new { id = sprintId });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> StartSprint(int id)
        {
            int? projectId = await _sprintService.GetProjectIdForSprintAsync(id);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            var success = await _sprintPlanningService.StartSprint(id);
            if (!success) return NotFound();

            return RedirectToAction("Index", "Sprint", new { id = (int)projectId });
        }
    }
}
