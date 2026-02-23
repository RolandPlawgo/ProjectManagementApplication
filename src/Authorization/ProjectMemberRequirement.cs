using Microsoft.AspNetCore.Authorization;

public class ProjectMemberRequirement : IAuthorizationRequirement
{
    public int ProjectId { get; }

    public ProjectMemberRequirement(int projectId)
    {
        ProjectId = projectId;
    }
}