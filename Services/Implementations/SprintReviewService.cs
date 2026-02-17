using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.SprintReviewDtos;
using ProjectManagementApplication.Dto.Requests.SprintReviewRequests;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Services.Implementations
{
    public class SprintReviewService : ISprintReviewService
    {
        private readonly ApplicationDbContext _context;

        public SprintReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SprintReviewDto?> GetSprintReviewAsync(int id)
        {
            Sprint? sprint = await _context.Sprints.Where(s => s.Id == id)
                .Include(s => s.Project)
                .FirstOrDefaultAsync();
            if (sprint == null) return null;

            var productStories = await _context.UserStories.Where(u => u.SprintId == id && u.Status == Status.Backlog)
                .Include(u => u.Epic)
            .Include(u => u.Subtasks)
                .ToListAsync();
            var sprintStories = await _context.UserStories.Where(u => u.SprintId == id && u.Status == Status.Sprint)
                .Include(u => u.Epic)
            .Include(u => u.Subtasks)
                .ToListAsync();
            var productIncrementStories = await _context.UserStories.Where(u => u.SprintId == id && u.Status == Status.ProductIncrement)
                .Include(u => u.Epic)
                .Include(u => u.Subtasks)
                .ToListAsync();

            List<UserStoryDto> backlogStoriesVm = new();
            foreach (var story in productStories)
            {
                backlogStoriesVm.Add(new UserStoryDto()
                {
                    Id = story.Id,
                    Title = story.Title,
                    EpicTitle = story.Epic.Title,
                    AllTasksCount = story.Subtasks.Count(),
                    CompletedTasksCount = story.Subtasks.Where(s => s.Done).Count()
                });
            }
            List<UserStoryDto> sprintStoriesVm = new();
            foreach (var story in sprintStories)
            {
                sprintStoriesVm.Add(new UserStoryDto()
                {
                    Id = story.Id,
                    Title = story.Title,
                    EpicTitle = story.Epic.Title,
                    AllTasksCount = story.Subtasks.Count(),
                    CompletedTasksCount = story.Subtasks.Where(s => s.Done).Count()
                });
            }
            List<UserStoryDto> productIncrementStoriesVm = new();
            foreach (var story in productIncrementStories)
            {
                productIncrementStoriesVm.Add(new UserStoryDto()
                {
                    Id = story.Id,
                    Title = story.Title,
                    EpicTitle = story.Epic.Title,
                    AllTasksCount = story.Subtasks.Count(),
                    CompletedTasksCount = story.Subtasks.Where(s => s.Done).Count()
                });
            }

            var dto = new SprintReviewDto()
            {
                SprintId = id,
                ProjectId = sprint.ProjectId,
                ProjectName = sprint.Project.Name,
                SprintGoal = sprint.SprintGoal,
                ProductBacklogUserStories = backlogStoriesVm,
                SprintBacklogUserStories = sprintStoriesVm,
                ProductIncrementUserStories = productIncrementStoriesVm
            };

            return dto;
        }

        public async Task<bool> MoveCardAsync(MoveCardRequest moveCardRequest)
        {
            UserStory? userStory = await _context.UserStories
                .Where(u => u.Id == moveCardRequest.UserStoryId)
                .Include(u => u.Sprint)
                .FirstOrDefaultAsync();
            if (userStory == null) return false;

            if (moveCardRequest.TargetList == TargetList.ProductBacklog)
            {
                userStory.Status = Status.Backlog;
            }
            if (moveCardRequest.TargetList == TargetList.SprintBacklog)
            {
                userStory.Status = Status.Sprint;
            }
            if (moveCardRequest.TargetList == TargetList.ProductIncrement)
            {
                userStory.Status = Status.ProductIncrement;
            }

            _context.UserStories.Update(userStory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> FinishSprintAsync(int id)
        {
            Sprint? sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return false;

            sprint.Active = false;

            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
