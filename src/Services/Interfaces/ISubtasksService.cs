using ProjectManagementApplication.Dto.Read.SubtasksDtos;
using ProjectManagementApplication.Dto.Requests.SubtasksRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface ISubtasksService
    {
        public Task CreateSubtaskAsync(CreateSubtaskRequest request);
        public Task<SubtaskDetailsDto?> GetDetailsAsync(int id);
        public Task<EditSubtaskDto?> GetForEditAsync(int id);
        public Task<bool> EditSubtaskAsync(EditSubtaskRequest request);
        public Task<bool> DeleteSubtaskAsync(int id);
        public Task AddCommentAsync(AddCommentRequest request);
    }
}
