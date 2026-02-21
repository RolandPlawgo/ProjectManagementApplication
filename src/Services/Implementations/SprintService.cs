using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Services.Implementations
{
    public class SprintService : ISprintService
    {
        private readonly ApplicationDbContext _context;
        public SprintService(ApplicationDbContext context) 
        {
            _context = context;
        }


        public async Task<bool?> IsSprintActiveAsync(int sprintId)
        {
            var sprint = await _context.Sprints.FindAsync(sprintId);
            if (sprint == null) return null;
            return sprint.Active;
        }

        public async Task<int?> GetProjectIdForSprintAsync(int sprintId)
        {
            var sprint = await _context.Sprints.FindAsync(sprintId);
            if (sprint == null) return null;
            return sprint.ProjectId;
        }

        public async Task<bool?> IsSprintFinishedAsync(int sprintId)
        {
            var sprint = await _context.Sprints.FindAsync(sprintId);
            if (sprint == null) return null;
            return sprint.EndDate.HasValue && sprint.EndDate < DateTime.Now;
        }

        public async Task<int?> GetSprintIdForSubtaskAsync(int subtaskId)
        {
            var subtask = await _context.Subtasks.FindAsync(subtaskId);
            if (subtask == null) return null;
            var userStory = await _context.UserStories.FindAsync(subtask.UserStoryId);
            if (userStory == null) return null;
            return userStory.SprintId;
        }
        public async Task<int?> GetSprintIdForUserStoryAsync(int userStoryId)
        {
            var userStory = await _context.UserStories.FindAsync(userStoryId);
            if (userStory == null) return null;
            return userStory.SprintId;
        }

        public async Task<int?> GetActiveSprintId(int projectId)
        {
            Sprint? sprint = await _context.Sprints.Where(s => s.ProjectId == projectId && s.Active == true && s.EndDate.HasValue && s.EndDate > DateTime.Now).FirstOrDefaultAsync();
            if (sprint == null) return null;
            return sprint.Id;
        }

        public async Task<int?> GetCompletedSprintId(int projectId)
        {
            Sprint? sprint = await _context.Sprints.Where(s => s.ProjectId == projectId && s.Active == true && s.EndDate.HasValue && s.EndDate <= DateTime.Now).FirstOrDefaultAsync();
            if (sprint == null) return null;
            return sprint.Id;
        } 
        public async Task<int?> GetOrCreateNewSprintAsync(int projectId)
        {
            Project? project = await _context.Projects.FindAsync(projectId);
            if (project == null) return null;
            Sprint? sprint = await _context.Sprints.Where(s => s.ProjectId == projectId && s.Active == false && s.EndDate == null).FirstOrDefaultAsync();
            if (sprint == null)
            {
                Sprint newSprint = new Sprint()
                {
                    Project = project,
                    Active = false,
                    SprintGoal = ""
                };
                var createdSprint = await _context.Sprints.AddAsync(newSprint);
                await _context.SaveChangesAsync();
                sprint = createdSprint.Entity;
            }

            return sprint.Id;
        }
    }
}
