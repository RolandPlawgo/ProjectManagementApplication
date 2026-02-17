using ProjectManagementApplication.Dto.Read.ScrumBoardDtos;
using ProjectManagementApplication.Dto.Requests.ScrumBoardRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IScrumBoardService
    {
        public Task<ScrumBoardDto?> GetScrumBoardAsync(int sprintId);
        public Task<bool> MoveCardAsync(MoveCardRequest moveCardRequest);
        public Task<bool> FinishSprintEarlyAsync(int id);
    }
}
