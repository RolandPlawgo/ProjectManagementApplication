using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Common;
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
        private readonly ISprintService _sprintService;

        public SprintReviewService(ApplicationDbContext context, ISprintService sprintService)
        {
            _context = context;
            _sprintService = sprintService;
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

        public async Task<Result> MoveCardAsync(MoveCardRequest moveCardRequest)
        {
            UserStory? userStory = await _context.UserStories
                .Where(u => u.Id == moveCardRequest.UserStoryId)
                .Include(u => u.Sprint)
                .FirstOrDefaultAsync();
            if (userStory == null) return Result.NotFound("User story not found.");

            int? sprintId = userStory.SprintId;
            if (sprintId == null) return Result.ValidationFailed("User story is not assigned to a sprint.");

            bool? isSprintActive = await _sprintService.IsSprintActiveAsync((int)sprintId);
            if (isSprintActive == null) return Result.NotFound();
            if (!(bool)isSprintActive)
                return Result.ValidationFailed("Cannot move card because the sprint is active.");

            bool? isSprintFinished = await _sprintService.IsSprintFinishedAsync((int)sprintId);
            if (isSprintFinished == null) return Result.NotFound();
            if (!(bool)isSprintFinished)
                return Result.ValidationFailed("Cannot move card because the sprint is not finished.");

            if (moveCardRequest.TargetList == TargetList.ProductBacklog)
            {
                if (userStory.Status == Status.Backlog) return Result.ValidationFailed("User story is already in the product backlog.");
                userStory.Status = Status.Backlog;
            }
            if (moveCardRequest.TargetList == TargetList.SprintBacklog)
            {
                if (userStory.Status == Status.Sprint) return Result.ValidationFailed("User story is already in the sprint backlog.");
                userStory.Status = Status.Sprint;
            }
            if (moveCardRequest.TargetList == TargetList.ProductIncrement)
            {
                if (userStory.Status == Status.ProductIncrement) return Result.ValidationFailed("User story is already in product increment.");
                userStory.Status = Status.ProductIncrement;
            }

            _context.UserStories.Update(userStory);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> FinishSprintAsync(int id)
        {
            Sprint? sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return Result.NotFound("Sprint not found");

            bool? isSprintActive = await _sprintService.IsSprintActiveAsync(sprint.Id);
            if (isSprintActive == null) return Result.NotFound();
            if (!(bool)isSprintActive)
                return Result.ValidationFailed("Cannot finish sprint because the sprint is active.");

            bool? isSprintFinished = await _sprintService.IsSprintFinishedAsync(sprint.Id);
            if (isSprintFinished == null) return Result.NotFound();
            if (!(bool)isSprintFinished)
                return Result.ValidationFailed("Cannot complete sprint review because the sprint is not finished.");

            if (!await AllUserSotiesAcceptedOrRejected(id))
                return Result.ValidationFailed("All user stories must be accepted or rejected before finishing the sprint.");

            sprint.Active = false;

            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }

        private async Task<bool> AllUserSotiesAcceptedOrRejected(int sprintId)
        {
            var userStories = await _context.UserStories.Where(u => u.SprintId == sprintId && u.Status == Status.Sprint).ToListAsync();
            return userStories.Count() == 0;
        }
    }
}
