using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.BacklogDtos;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Services.Implementations
{
    public class BacklogService : IBacklogService
    {
        private readonly ApplicationDbContext _context;
        public BacklogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<BacklogDto?> GetBacklogAsync(int projectId)
        {
            var project = await _context.Projects
            .Include(p => p.Users)
            .Include(p => p.Epics)
                .ThenInclude(e => e.UserStories
                    .Where(u => u.Status == Status.Backlog)
                    .OrderByDescending(u => u.Id))
            .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return null;

            var dto = new BacklogDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Epics = project.Epics.Select(e => new EpicSummaryDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    UserStories = e.UserStories.Select(us => new UserStorySummaryDto
                    {
                        Id = us.Id,
                        Title = us.Title
                    }).ToList()
                }).ToList()
            };

            return dto;
        }
    }
}
