using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;

public class ProjectMemberHandler
    : AuthorizationHandler<ProjectMemberRequirement>
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _um;

    public ProjectMemberHandler(ApplicationDbContext db,
                                UserManager<ApplicationUser> um)
    {
        _db = db;
        _um = um;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectMemberRequirement requirement)
    {
        // 1) get the current user id
        var userId = _um.GetUserId(context.User);
        if (userId == null) return;

        // 2) load the project *and* its Users
        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Id == requirement.ProjectId);
        if (project == null) return;

        // 3) succeed if the user is in the project
        if (project.Users.Any(u => u.Id == userId))
        {
            context.Succeed(requirement);
        }
    }
}
