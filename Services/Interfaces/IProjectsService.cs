using ProjectManagementApplication.Dto.Read.ProjectsDtos;
using ProjectManagementApplication.Dto.Requests.ProjectsRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IProjectsService
    {
        public Task<List<ProjectCardDto>> GetProjectsForUserAsync(string userId);
        public Task<ProjectDetailsDto?> GetProjectDetailsAsync(int projectId);
        public Task<bool> UpdateProjectAsync(EditProjectRequest editProjectRequest);
        public Task CreateProjectAsync(CreateProjectRequest createProjectRequest);
        public Task<bool> DeleteProjectAsync(int projectId);
    }
}
