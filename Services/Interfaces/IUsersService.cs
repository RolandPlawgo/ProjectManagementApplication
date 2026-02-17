using ProjectManagementApplication.Dto.Read.UsersDtos;
using ProjectManagementApplication.Dto.Requests.UsersRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IUsersService
    {
        public Task<List<UsersDto>> GetUsersAsync(int page, int pageSize);
        public Task<string?> CreateUserWithTempPasswordAsync(CreateUserRequest request);
        public Task<EditUserDto?> GetForEditAsync(string id);
        public Task<bool> EditUserAsync(EditUserRequest request);
    }
}
