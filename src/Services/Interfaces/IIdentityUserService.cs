using ProjectManagementApplication.Authentication;
using System.Security.Claims;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IIdentityUserService
    {
        public Task<List<ApplicationUser>> GetAllUsersAsync();
        public Task<ApplicationUser?> FindByIdAsync(string id);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        public string? GetUserId(ClaimsPrincipal userPrincipal);
        Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal userPrincipal);
    }
}
