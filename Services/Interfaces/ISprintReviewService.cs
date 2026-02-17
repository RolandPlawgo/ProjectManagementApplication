
using ProjectManagementApplication.Dto.Read.SprintReviewDtos;
using ProjectManagementApplication.Dto.Requests.SprintReviewRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface ISprintReviewService
    {
        public Task<SprintReviewDto?> GetSprintReviewAsync(int id);
        public Task<bool> MoveCardAsync(MoveCardRequest moveCardRequest);
        public Task<bool> FinishSprintAsync(int id);
    }
}
