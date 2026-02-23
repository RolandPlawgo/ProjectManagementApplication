using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Dto.Requests.EpicsViewModels;
using ProjectManagementApplication.Models.EpicViewModels;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Controllers
{
    [Authorize(Roles = "Product Owner")]
    public class EpicsController : Controller
    {
        private readonly IEpicsService _epicsService;
        private readonly IAuthorizationService _authorizationService;

        public EpicsController(ApplicationDbContext context, IEpicsService epicsService, IAuthorizationService authorizationService)
        {
            _epicsService = epicsService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int projectId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(projectId));
            if (!authResult.Succeeded) return Forbid();

            var vm = new CreateEpicViewModel { ProjectId = projectId };

            return PartialView("_CreateEpic", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEpicViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEpic", model);

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(model.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            try
            {
                await _epicsService.CreateEpicAsync(new CreateEpicRequest
                {
                    ProjectId = model.ProjectId,
                    Title = model.Title
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "An error occured while creating the epic." });
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var epic = await _epicsService.GetEpicForEditAsync(id);
            if (epic == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(epic.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            var vm = new EditEpicViewModel
            {
                Id = epic.Id,
                ProjectId = epic.ProjectId,
                Title = epic.Title
            };
            return PartialView("_EditEpic", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEpicViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditEpic", model);

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(model.ProjectId));
            if (!authResult.Succeeded) return Forbid();

            try
            {
                bool success = await _epicsService.EditEpicAsync(new EditEpicRequest
                {
                    Id = model.Id,
                    Title = model.Title
                });
                if (success == false)
                {
                    return NotFound();
                }
            }
            catch
            {
                return Json(new { success = false, error = "An error occured while saving the epic." });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? projectId = await _epicsService.GetProjectIdForEpicAsync(id);
            if (projectId == null) return Json(new { success = false });
            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            try
            {
                bool success = await _epicsService.DeleteEpicAsync(id);
                if (success == false)
                {
                    return Json(new { success = false });
                }
            }
            catch
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }
    }
}
