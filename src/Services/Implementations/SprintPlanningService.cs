using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.SprintPlanningDtos;
using ProjectManagementApplication.Dto.Requests.SprintPlanningRequests;
using ProjectManagementApplication.Services.Interfaces;
using SQLitePCL;

namespace ProjectManagementApplication.Services.Implementations
{
    public class SprintPlanningService : ISprintPlanningService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISprintService _sprintService;
        public SprintPlanningService(ApplicationDbContext context, ISprintService sprintService)
        {
            _context = context;
            _sprintService = sprintService;
        }

        public async Task<SprintPlanningDto?> GetSprintPlanningAsync(int sprintId)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == sprintId);
            if (sprint == null) return null;

            var backlogStories = await _context.UserStories
                .Where(u => u.Status == Status.Backlog)
                .Include(u => u.Epic).Include(u => u.Subtasks)
                .ToListAsync();
            var sprintStories = await _context.UserStories
                .Where(u => u.Status == Status.Sprint && u.SprintId == sprintId)
                .Include(u => u.Epic).Include(u => u.Subtasks)
                .ToListAsync();

            var dto = new SprintPlanningDto
            {
                SprintId = sprint.Id,
                SprintGoal = sprint.SprintGoal,
                ProjectId = sprint.ProjectId,
                ProjectName = sprint.Project.Name,
                BacklogUserStories = backlogStories.Select(us => new UserStorySummaryDto
                {
                    Id = us.Id,
                    Title = us.Title,
                    EpicTitle = us.Epic.Title
                }).ToList(),
                SprintUserStories = sprintStories.Select(us => new UserStorySummaryDto
                {
                    Id = us.Id,
                    Title = us.Title,
                    EpicTitle = us.Epic.Title
                }).ToList(),
                Subtasks = sprintStories.SelectMany(us => us.Subtasks).Select(st => new SubtaskSummaryDto
                {
                    Id = st.Id,
                    Title = st.Title,
                    UserStoryId = st.UserStoryId
                }).Union(backlogStories.SelectMany(us => us.Subtasks).Select(st => new SubtaskSummaryDto
                {
                    Id = st.Id,
                    Title = st.Title,
                    UserStoryId = st.UserStoryId
                })).ToList()
            };

            return dto;
        }

        public async Task<Result> MoveUserStory(MoveUserStoryRequest moveUserStoryRequest)
        {
            bool? isSprintActive = await _sprintService.IsSprintActiveAsync(moveUserStoryRequest.SprintId);
            if (isSprintActive.HasValue)
            {
                if ((bool)isSprintActive)
                    return Result.ValidationFailed("The sprint has already started.");
            }
            else
                return Result.NotFound(); 

            var userStory = await _context.UserStories.Where(u => u.Id == moveUserStoryRequest.UserStoryId)
                .Include(u => u.Epic)
                .FirstOrDefaultAsync();
            if (userStory == null) return Result.NotFound("The user story has not been found.");

            if (moveUserStoryRequest.TargetStatus == Status.Sprint && userStory.Status == Status.Backlog)
            {
                userStory.Status = Status.Sprint;
                userStory.SprintId = moveUserStoryRequest.SprintId;
            }
            else if (moveUserStoryRequest.TargetStatus == Status.Backlog && userStory.Status == Status.Sprint)
            {
                userStory.Status = Status.Backlog;
                userStory.SprintId = null;
            }

            _context.Update(userStory);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> SetSprintGoalAsync(int sprintId, string newSprintGoal)
        {
            bool? isSprintActive = await _sprintService.IsSprintActiveAsync(sprintId);
            if (isSprintActive.HasValue)
            {
                if ((bool)isSprintActive)
                    return Result.ValidationFailed("The sprint has already started.");
            }
            else
                return Result.NotFound();

            var sprint = await _context.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId);
            if (sprint == null) return Result.NotFound("The sprint has not been found.");
            sprint.SprintGoal = newSprintGoal;
            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> StartSprint(int id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sprint == null) return Result.NotFound("The sprint has not been found.");

            bool? isSprintActive = await _sprintService.IsSprintActiveAsync(sprint.Id);
            if (isSprintActive.HasValue)
            {
                if ((bool)isSprintActive)
                    return Result.ValidationFailed("The sprint has already started.");
            }
            else
                return Result.NotFound();

            sprint.Active = true;
            sprint.StartDate = DateTime.Now;
            sprint.EndDate = DateTime.Now.AddDays(sprint.Project.SprintDuration * 7);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }
    }
}
