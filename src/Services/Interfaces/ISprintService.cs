namespace ProjectManagementApplication.Services.Interfaces
{
    public interface ISprintService
    {
        public Task<int?> GetActiveSprintId(int projectId);
        public Task<int?> GetCompletedSprintId(int projectId);
        public Task<int?> GetOrCreateNewSprintAsync(int projectId);
        public Task<bool?> IsSprintActiveAsync(int sprintId);
        public Task<int?> GetProjectIdForSprintAsync(int sprintId);
        public Task<bool?> IsSprintFinishedAsync(int sprintId);
        public Task<int?> GetSprintIdForSubtaskAsync(int subtaskId);
        public Task<int?> GetSprintIdForUserStoryAsync(int userStoryId);
    }
}
