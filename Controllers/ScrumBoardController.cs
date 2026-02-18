
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Dto.Read.ScrumBoardDtos;
using ProjectManagementApplication.Dto.Requests.ScrumBoardRequests;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Controllers
{
    public class ScrumBoardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IScrumBoardService _scrumBoardService;
        private readonly ISprintService _sprintService;

        public ScrumBoardController(UserManager<ApplicationUser> userManager, 
            IAuthorizationService authorizationService,
            IScrumBoardService scrumBoardService,
            ISprintService sprintService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _scrumBoardService = scrumBoardService;
            _sprintService = sprintService;
        }

        public async Task<IActionResult> Index(int id)
        {
            int? projectId = await _sprintService.GetProjectIdForSprintAsync(id);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            bool? isSprintActive = await _sprintService.IsSprintActiveAsync(id);
            bool? isSprintFinished = await _sprintService.IsSprintFinishedAsync(id);
            if (isSprintActive == null || isSprintFinished == null) return NotFound();
            if (!(bool)isSprintActive || (bool)isSprintFinished)
            {
                return RedirectToAction("Index", "Sprint", new { id = projectId });
            }

            ScrumBoardDto? dto = await _scrumBoardService.GetScrumBoardAsync(id);
            if (dto == null) return NotFound();

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> MoveCard(int id, string targetList)
        {
            int? sprintId = await _sprintService.GetSprintIdForSubtaskAsync(id);
            if (sprintId == null) return Json(new { success = false });
            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return Json(new { success = false });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Json(new { success = false });

            if (!Enum.TryParse<TargetList>(targetList, out TargetList parsedTargetList))
                return Json(new { success = false });
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json( new { success = false });

            var moveCardRequest = new MoveCardRequest()
            {
                SubtaskId = id,
                TargetList = parsedTargetList,
                CurrentUserId = user.Id
            };

            try
            {
                var success = await _scrumBoardService.MoveCardAsync(moveCardRequest);
                if (!success) return Json(new { success = false });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }

            string initials = "";
            initials = Helpers.ApplicationUserHelper.UserInitials(user);

            return Json(new { success = true, initials });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> FinishSprintEarly(int id)
        {
            int? projectId = await _sprintService.GetProjectIdForSprintAsync(id);
            if (projectId == null) return NotFound();
            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            bool success = await _scrumBoardService.FinishSprintEarlyAsync(id);
            if (!success) return NotFound();

            return RedirectToAction("Index", "SprintReview", new { id });
        }
    }
}
