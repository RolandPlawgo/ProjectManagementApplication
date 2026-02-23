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
        var userId = _um.GetUserId(context.User);
        if (userId == null) return;

        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Users)
            .FirstOrDefaultAsync(p => p.Id == requirement.ProjectId);
        if (project == null) return;

        if (project.Users.Any(u => u.Id == userId))
        {
            context.Succeed(requirement);
        }
    }
}
