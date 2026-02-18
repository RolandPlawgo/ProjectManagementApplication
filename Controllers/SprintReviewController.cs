using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjectManagementApplication.Dto.Read.SprintReviewDtos;
using ProjectManagementApplication.Services.Interfaces;
using ProjectManagementApplication.Dto.Requests.SprintReviewRequests;

namespace ProjectManagementApplication.Controllers
{
    public class SprintReviewController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISprintService _sprintService;
        private readonly ISprintReviewService _sprintReviewService;

        public SprintReviewController(IAuthorizationService authorizationService, ISprintService sprintService, ISprintReviewService sprintReviewService)
        {
            _authorizationService = authorizationService;
            _sprintService = sprintService;
            _sprintReviewService = sprintReviewService;
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
            if (!(bool)isSprintActive || !(bool)isSprintFinished)
            {
                return RedirectToAction("Index", "Sprint", new { id = projectId });
            }

            SprintReviewDto? dto = await _sprintReviewService.GetSprintReviewAsync(id);
            if (dto == null) return NotFound();
            
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveCard(int id, string targetList)
        {
            int? sprintId = await _sprintService.GetSprintIdForUserStoryAsync(id);
            if (sprintId == null) return NotFound();

            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            if (!Enum.TryParse<TargetList>(targetList, out TargetList parsedTargetList))
                return Json(new { success = false });

            var moveCardRequest = new MoveCardRequest()
            {
                UserStoryId = id,
                TargetList = parsedTargetList
            };
            try
            {
                bool success = await _sprintReviewService.MoveCardAsync(moveCardRequest);
                if (!success) return Json(new { success = false });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Product Owner")]
        public async Task<IActionResult> FinishSprint(int id)
        {
            int? projectId = await _sprintService.GetProjectIdForSprintAsync(id);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            bool success = await _sprintReviewService.FinishSprintAsync(id);
            if (!success) return NotFound();

            return RedirectToAction("Index", "ProductIncrement", new {id = projectId});
        }
    }
}
