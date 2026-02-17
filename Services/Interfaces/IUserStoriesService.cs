using ProjectManagementApplication.Dto.Read.UserStoriesDtos;
using ProjectManagementApplication.Dto.Requests.UserStoriesRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IUserStoriesService
    {
        public Task<bool> CreateUserStoryAsync(CreateUserStoryRequest createUserStoryRequest);
        public Task<UserStoryDetailsDto?> GetUserStoryDetailsAsync(int userStoryId);
        public Task<EditUserStoryDto?> GetForEditAsync(int id);
        public Task<int?> GetProjectIdForUserStoryAsync(int userStoryId);
        public Task<bool> EditUserStoriyAsync(EditUserStoryRequest editUserStoryRequest);
        public Task<bool> DeleteUserStoryAsync(int id);
    }
}
