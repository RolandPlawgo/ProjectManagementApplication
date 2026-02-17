using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Dto.Read.SubtasksDtos;
using ProjectManagementApplication.Dto.Requests.SubtasksRequests;
using ProjectManagementApplication.Models.Subtasks;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Controllers
{
    public class SubtasksController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISprintService _sprintService;
        private readonly ISubtasksService _subtasksService;

        public SubtasksController(
            UserManager<ApplicationUser> userManager,
            IAuthorizationService authorizationService,
            ISprintService sprintService,
            ISubtasksService subtasksService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _sprintService = sprintService;
            _subtasksService = subtasksService;
        }

        // GET: /Subtasks/Create?storyId=5
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int storyId)
        {
            int? sprintId = await _sprintService.GetSprintIdForUserStoryAsync(storyId);
            if (sprintId == null) return NotFound();
            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            var vm = new CreateSubtaskViewModel { UserStoryId = storyId };
            return PartialView("_CreateSubtask", vm);
        }

        // POST: /Subtasks/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateSubtaskViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateSubtask", model);

            var sprintId = await _sprintService.GetSprintIdForUserStoryAsync(model.UserStoryId);
            if (sprintId == null) return Json(new { success = false, error = "User story not found." });
            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return Json(new { success = false, error = "Project not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            var subtask = new CreateSubtaskRequest
            {
                Title = model.Title,
                Content = model.Content ?? "",
                UserStoryId = model.UserStoryId
            };
            try
            {
                await _subtasksService.CreateSubtaskAsync(subtask);
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "An error occured while creating the task." });
            }

            return Json(new { success = true });
        }

        // GET: /Subtasks/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            int? sprintId = await _sprintService.GetSprintIdForSubtaskAsync(id);
            if (sprintId == null) return NotFound();
            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            EditSubtaskDto? dto = await _subtasksService.GetForEditAsync(id);
            if (dto == null) return NotFound();

            var vm = new EditSubtaskViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                Content = dto.Content,
                UserStoryId = dto.UserStoryId
            };
            return PartialView("_EditSubtask", vm);
        }

        // POST: /Subtasks/Edit/5
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(EditSubtaskViewModel vm)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditSubtask", vm);

            int? sprintId = await _sprintService.GetSprintIdForSubtaskAsync(vm.Id); 
            if (sprintId == null) return Json(new { success = false, error = "User story not found." });
            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return Json(new { success = false, error = "Project not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            var request = new EditSubtaskRequest()
            {
                Id = vm.Id,
                Title = vm.Title,
                Content = vm.Content,
                UserStoryId = vm.UserStoryId
            };
            bool success = await _subtasksService.EditSubtaskAsync(request);
            if (!success) return NotFound();

            return Json(new { success = true });
        }

        // POST: /Subtasks/Delete/5
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int? sprintId = await _sprintService.GetSprintIdForSubtaskAsync(id);
            if (sprintId == null) return Json(new { success = false, error = "User story not found." });
            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return Json(new { success = false, error = "Project not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            bool success = await  _subtasksService.DeleteSubtaskAsync(id);
            if (!success) return NotFound();

            return Json(new { success = true });
        }

        // GET: /Subtasks/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            int? sprintId = await _sprintService.GetSprintIdForSubtaskAsync(id);
            if (sprintId == null) return NotFound();
            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            SubtaskDetailsDto? dto = await _subtasksService.GetDetailsAsync(id);
            if (dto == null) return NotFound();

            return PartialView("_SubtaskDetails", dto);
        }

        // POST: /Subtasks/AddComment
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int taskId, string content)
        {
            int? sprintId = await _sprintService.GetSprintIdForSubtaskAsync(taskId);
            if (sprintId == null) return NotFound();
            int? projectId = await _sprintService.GetProjectIdForSprintAsync((int)sprintId);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var comment = new AddCommentRequest
            {
                SubtaskId = taskId,
                Content = content,
                Author = user
            };
            
            await _subtasksService.AddCommentAsync(comment);

            SubtaskDetailsDto? dto = await _subtasksService.GetDetailsAsync(taskId);
            if (dto == null) return NotFound();

            return PartialView("_SubtaskDetails", dto);
        }
    }
}
