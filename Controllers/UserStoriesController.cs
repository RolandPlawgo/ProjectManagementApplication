using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagementApplication.Dto.Read.UserStoriesDtos;
using ProjectManagementApplication.Dto.Requests.UserStoriesRequests;
using ProjectManagementApplication.Models.UserStoryViewModels;
using ProjectManagementApplication.Services.Interfaces;

public class UserStoriesController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserStoriesService _userStoriesService;
    private readonly IEpicsService _epicsService;

    public UserStoriesController(IAuthorizationService authorizationService, IUserStoriesService userStoriesService, IEpicsService epicsService)
    {
        _authorizationService = authorizationService;
        _userStoriesService = userStoriesService;        _epicsService = epicsService;

    }

    // GET: /UserStories/Create?epicId=5
    [HttpGet]
    [Authorize(Roles = "Product Owner")]
    public async Task<IActionResult> Create(int epicId)
    {
        int? projectId = await _epicsService.GetProjectIdForEpicAsync(epicId);
        if (projectId == null) return NotFound();
        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
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

        int? projectId = await _epicsService.GetProjectIdForEpicAsync(model.EpicId);
        if (projectId == null) return NotFound();
        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
        if (!authResult.Succeeded) return Forbid();

        bool success = await _userStoriesService.CreateUserStoryAsync(new CreateUserStoryRequest
        {
            EpicId = model.EpicId,
            Title = model.Title,
            Description = model.Description
        });
        if (success == false)
        {
            return NotFound();
        }

        return Json(new { success = true });
    }

    // GET: /UserStories/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        int? projectId = await _userStoriesService.GetProjectIdForUserStoryAsync(id);
        if (projectId == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
        if (!authResult.Succeeded) return Forbid();

        UserStoryDetailsDto? userStoryDetails = await _userStoriesService.GetUserStoryDetailsAsync(id);

        return PartialView("_UserStoryDetails", userStoryDetails);
    }

    // GET: /UserStories/Edit/5
    [HttpGet]
    [Authorize(Roles = "Product Owner")]
    public async Task<IActionResult> Edit(int id)
    {
        int? projectId = await _userStoriesService.GetProjectIdForUserStoryAsync(id);
        if (projectId == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
        if (!authResult.Succeeded) return Forbid();

        EditUserStoryDto? userStory = await _userStoriesService.GetForEditAsync(id);
        if (userStory == null) return NotFound();

        var vm = new EditUserStoryViewModel
        {
            Id = userStory.Id,
            EpicId = userStory.EpicId,
            Title = userStory.Title,
            Description = userStory.Description,
            Epics = userStory.Epics.Select(e => new SelectListItem
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

        int? projectId = await _userStoriesService.GetProjectIdForUserStoryAsync(vm.Id);
        if (projectId == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
        if (!authResult.Succeeded) return Forbid();

        var editUserStoryRequest = new EditUserStoryRequest
        {
            Id = vm.Id,
            EpicId = vm.EpicId,
            Title = vm.Title,
            Description = vm.Description
        };
        bool success = await _userStoriesService.EditUserStoriyAsync(editUserStoryRequest);
        if (!success)
            return NotFound();

        return Json(new { success = true });
    }

    // POST: /UserStories/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Product Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            int? projectId = await _userStoriesService.GetProjectIdForUserStoryAsync(id);
            if (projectId == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement((int)projectId));
            if (!authResult.Succeeded) return Forbid();

            bool success = await _userStoriesService.DeleteUserStoryAsync(id);
            if (!success) return Json(new { success = false });
        }
        catch
        {
            return Json(new { success = false });
        }
        return Json(new { success = true });
    }
}
