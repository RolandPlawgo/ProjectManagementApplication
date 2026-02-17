using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.ProductIncrementDtos;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Services.Implementations
{
    public class ProductIncrementService : IProductIncrementService
    {
        private readonly ApplicationDbContext _context;
        public ProductIncrementService(ApplicationDbContext context) 
        {
            _context = context;
        }
        public async Task<ProductIncrementDto?> GetProductIncrementAsync(int projectId)
        {
            Project? project = await _context.Projects.Where(p => p.Id == projectId)
                .Include(p => p.Epics.OrderByDescending(e => e.Id)).ThenInclude(e => e.UserStories)
                .FirstOrDefaultAsync();
            if (project == null) return null;

            List<SprintSummaryDto> sprintsDto = new List<SprintSummaryDto>();
            List<Sprint> sprints = await _context.Sprints.Where(s => s.ProjectId == projectId && s.Active == false && s.EndDate < DateTime.Now)
                .Include(s => s.UserStories.Where(u => u.Status == Status.ProductIncrement))
                    .ThenInclude(u => u.Epic)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
            foreach (Sprint sprint in sprints)
            {
                List<UserStorySummaryDto> storiesDto = new List<UserStorySummaryDto>();
                foreach (UserStory story in sprint.UserStories)
                {
                    storiesDto.Add(new UserStorySummaryDto()
                    {
                        Id = story.Id,
                        Title = story.Title,
                        EpicTitle = story.Epic.Title
                    });
                }
                sprintsDto.Add(new SprintSummaryDto()
                {
                    Id = sprint.Id,
                    SprintGoal = sprint.SprintGoal,
                    UserStories = storiesDto
                });
            }

            List<EpicSummaryDto> epicsDto = new List<EpicSummaryDto>();
            foreach (Epic epic in project.Epics)
            {
                List<UserStorySummaryDto> storiesDto = new List<UserStorySummaryDto>();
                foreach (UserStory story in epic.UserStories)
                {
                    storiesDto.Add(new UserStorySummaryDto()
                    {
                        Id = story.Id,
                        Title = story.Title,
                        EpicTitle = story.Epic.Title
                    });
                }
                epicsDto.Add(new EpicSummaryDto()
                {
                    Id = epic.Id,
                    Title = epic.Title,
                    UserStories = storiesDto
                });
            }

            ProductIncrementDto dto = new ProductIncrementDto()
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                Sprints = sprintsDto,
                Epics = epicsDto
            };

            return dto;
        }
    }
}
