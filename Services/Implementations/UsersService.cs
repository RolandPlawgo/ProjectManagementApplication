using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Dto.Read.UsersDtos;
using ProjectManagementApplication.Dto.Requests.UsersRequests;
using ProjectManagementApplication.Helpers;
using ProjectManagementApplication.Models.UsersViewModels;
using ProjectManagementApplication.Services.Interfaces;
using System.Data;

namespace ProjectManagementApplication.Services.Implementations
{
    public class UsersService : IUsersService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UsersService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<UsersDto>> GetUsersAsync(int page, int pageSize)
        {
            var users = await _userManager.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<UsersDto> dto = new List<UsersDto>();
            foreach (ApplicationUser user in users)
            {
                string role = "";
                List<string> roles = (await _userManager.GetRolesAsync(user)).ToList();
                if (!roles.Contains("Scrum Master") && !roles.Contains("Product Owner"))
                {
                    role = "Developer";
                }
                else
                {
                    role = roles.FirstOrDefault() ?? "";
                }

                dto.Add(new UsersDto()
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? ""
                });
            }

            return dto;
        }

        public async Task<string?> CreateUserWithTempPasswordAsync(CreateUserRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            string tempPassword = PasswordHelper.GeneratePassword(_userManager.Options.Password);

            var result = await _userManager.CreateAsync(user, tempPassword);

            if (result.Succeeded)
            {
                if (request.Role == "Scrum Master" || request.Role == "Product Owner")
                {
                    await _userManager.AddToRoleAsync(user, request.Role);
                }
                user.MustChangePassword = true;
                await _userManager.UpdateAsync(user);

                return tempPassword;
            }
            else
            {
                return null;
            }
        }

        public async Task<EditUserDto?> GetForEditAsync(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            EditUserDto dto = new EditUserDto()
            {
                Id = id,
                Email = user!.Email,
                FirstName = user!.FirstName,
                LastName = user!.LastName,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? ""
            };

            return dto;
        }

        public async Task<bool> EditUserAsync(EditUserRequest request)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                return false;
            }
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.UserName = request.Email;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (request.Role == "Scrum Master" || request.Role == "Product Owner")
            {
                await _userManager.AddToRoleAsync(user, request.Role);
            }

            await _userManager.UpdateAsync(user);

            return true;
        }
    }
}
