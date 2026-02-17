using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.ProjectsDtos;
using ProjectManagementApplication.Dto.Requests.ProjectsRequests;
using ProjectManagementApplication.Helpers;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Services.Implementations
{
    public class ProjectsService : IProjectsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProjectsService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<ProjectCardDto>> GetProjectsForUserAsync(string userId)
        {
            List<Project> projects = new List<Project>();

            projects = await _context.Projects
                .Include(p => p.Users)
                .Where(p => p.Users.Any(u => u.Id == userId))
                .ToListAsync();

            var cards = projects.Select(p => new ProjectCardDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                UserInitials = p.Users.Select(u => ApplicationUserHelper.UserInitials(u)).ToList()
            }).ToList();

            return cards;
        }

        public async Task<ProjectDetailsDto?> GetProjectDetailsAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return null;

            var dto = new ProjectDetailsDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                SprintDuration = project.SprintDuration
            };

            foreach (var user in project.Users)
            {
                var role = (await _userManager.GetRolesAsync(user)).DefaultIfEmpty("Developer").FirstOrDefault();
                dto.UsersWithRoles.Add(new UserWithRolesDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Role = role ?? ""
                });
            }

            return dto;
        }

        public async Task CreateProjectAsync(CreateProjectRequest createProjectRequest)
        {
            var project = new Project
            {
                Name = createProjectRequest.Name,
                Description = createProjectRequest.Description,
                SprintDuration = createProjectRequest.SprintDuration
            };

            foreach (var userId in createProjectRequest.UserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    project.Users.Add(user);
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateProjectAsync(EditProjectRequest editProjectRequest)
        {
            var project = await _context.Projects
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == editProjectRequest.Id);
            if (project == null) return false;

            project.Name = editProjectRequest.Name;
            project.Description = editProjectRequest.Description;
            project.SprintDuration = editProjectRequest.SprintDuration;

            project.Users.Clear();
            foreach (var userId in editProjectRequest.UserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    project.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProjectAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
