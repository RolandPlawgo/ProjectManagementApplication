using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApplication.Dto.Read.BacklogDtos;
using ProjectManagementApplication.Services.Interfaces;

public class BacklogController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBacklogService _backlogService;

    public BacklogController(
        IAuthorizationService authorizationService,
        IBacklogService backlogService)
    {
        _authorizationService = authorizationService;
        _backlogService = backlogService;
    }

    public async Task<IActionResult> Index(int id)
    {
        BacklogDto? backlog = await _backlogService.GetBacklogAsync(id);

        if (backlog == null) return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
        if (!authResult.Succeeded) return Forbid();

        return View(backlog);
    }
}
