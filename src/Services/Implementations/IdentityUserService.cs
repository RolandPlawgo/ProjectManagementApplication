using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Services.Interfaces;
using System.Security.Claims;

namespace ProjectManagementApplication.Services.Implementations
{
    public class IdentityUserService : IIdentityUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public IdentityUserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> FindByIdAsync(string id) =>
            await _userManager.FindByIdAsync(id);

        public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal userPrincipal) => 
            await _userManager.GetUserAsync(userPrincipal);

        public string? GetUserId(ClaimsPrincipal userPrincipal) => 
            _userManager.GetUserId(userPrincipal);

        public Task<IList<string>> GetRolesAsync(ApplicationUser user) =>
            _userManager.GetRolesAsync(user);
    }
}
