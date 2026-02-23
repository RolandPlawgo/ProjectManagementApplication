using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Dto.Requests.ProjectsRequests
{
    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SprintDuration { get; set; }
        public List<string> UserIds { get; set; } = new();
    }
}
