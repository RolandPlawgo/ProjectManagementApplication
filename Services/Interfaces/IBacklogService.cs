using ProjectManagementApplication.Dto.Read.BacklogDtos;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IBacklogService
    {
        public Task<BacklogDto?> GetBacklogAsync(int projectId);
    }
}