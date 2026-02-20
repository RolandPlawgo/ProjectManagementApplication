
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Dto.Read.SprintReviewDtos;
using ProjectManagementApplication.Dto.Requests.SprintReviewRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface ISprintReviewService
    {
        public Task<SprintReviewDto?> GetSprintReviewAsync(int id);
        public Task<Result> MoveCardAsync(MoveCardRequest moveCardRequest);
        public Task<Result> FinishSprintAsync(int id);
    }
}
