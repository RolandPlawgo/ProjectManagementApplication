using ProjectManagementApplication.Dto.Read.SprintPlanningDtos;
using ProjectManagementApplication.Dto.Requests.SprintPlanningRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface ISprintPlanningService
    {
        public Task<SprintPlanningDto?> GetSprintPlanningAsync(int sprintId);
        public Task<bool> MoveUserStory(MoveUserStoryRequest moveUserStoryRequest);
        public Task<bool> SetSprintGoalAsync(int sprintId, string newSprintGoal);
        public Task<bool> StartSprint(int id);
    }
}
