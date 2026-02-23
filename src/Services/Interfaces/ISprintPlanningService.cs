using ProjectManagementApplication.Common;
using ProjectManagementApplication.Dto.Read.SprintPlanningDtos;
using ProjectManagementApplication.Dto.Requests.SprintPlanningRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface ISprintPlanningService
    {
        public Task<SprintPlanningDto?> GetSprintPlanningAsync(int sprintId);
        public Task<Result> MoveUserStory(MoveUserStoryRequest moveUserStoryRequest);
        public Task<Result> SetSprintGoalAsync(int sprintId, string newSprintGoal);
        public Task<Result> StartSprint(int id);
    }
}
