
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.UserStoriesDtos;
using ProjectManagementApplication.Dto.Requests.UserStoriesRequests;
using ProjectManagementApplication.Models.UserStoryViewModels;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Services.Implementations
{
    public class UserStoriesService : IUserStoriesService
    {
        private readonly ApplicationDbContext _context;
        public UserStoriesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateUserStoryAsync(CreateUserStoryRequest createUserStoryRequest)
        {
            var epic = await _context.Epics
            .Include(e => e.Project)
                .ThenInclude(p => p.Users)
            .FirstOrDefaultAsync(e => e.Id == createUserStoryRequest.EpicId);
            if (epic == null) return false;

            var story = new UserStory
            {
                EpicId = createUserStoryRequest.EpicId,
                Title = createUserStoryRequest.Title,
                Description = createUserStoryRequest.Description,
                Status = Status.Backlog
            };
            _context.UserStories.Add(story);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UserStoryDetailsDto?> GetUserStoryDetailsAsync(int userStoryId)
        {
            UserStory? userStory = await _context.UserStories.Where(u => u.Id == userStoryId).Include(u => u.Epic).FirstOrDefaultAsync();
            if (userStory == null) return null;
            UserStoryDetailsDto? userStoryDto = new UserStoryDetailsDto
            {
                Id = userStory.Id,
                Title = userStory.Title,
                Description = userStory.Description,
                EpicTitle = userStory.Epic.Title
            };
            return userStoryDto;
        }

        public async Task<EditUserStoryDto?> GetForEditAsync(int id)
        {
            var userStory = await _context.UserStories
            .Include(u => u.Epic)
            .FirstOrDefaultAsync(u => u.Id == id);
            if (userStory == null) return null;

            var epics = await _context.Epics
                .Where(e => e.ProjectId == userStory.Epic.ProjectId)
                .ToListAsync();

            var dto = new EditUserStoryDto
            {
                Id = userStory.Id,
                EpicId = userStory.EpicId,
                Title = userStory.Title,
                Description = userStory.Description,
                Epics = epics.Select(e => new EpicSummaryDto
                {
                    Id = e.Id,
                    Title = e.Title
                }).ToList()
            };

            return dto;
        }

        public async Task<bool> EditUserStoriyAsync(EditUserStoryRequest editUserStoryRequest)
        {
            var userStory = await _context.UserStories
            .Include(u => u.Epic)
            .FirstOrDefaultAsync(u => u.Id == editUserStoryRequest.Id);
            if (userStory == null) return false;

            userStory.EpicId = editUserStoryRequest.EpicId;
            userStory.Title = editUserStoryRequest.Title;
            userStory.Description = editUserStoryRequest.Description;
            _context.Update(userStory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int?> GetProjectIdForUserStoryAsync(int userStoryId)
        {
            var userStory = await _context.UserStories
            .Include(u => u.Epic)
            .Include(u => u.Subtasks)
            .FirstOrDefaultAsync(u => u.Id == userStoryId);
            if (userStory == null) return null;

            return userStory.Epic.ProjectId;
        }

        public async Task<bool> DeleteUserStoryAsync(int id)
        {
            var userStory = await _context.UserStories
            .Include(u => u.Epic)
            .FirstOrDefaultAsync(u => u.Id == id);
            if (userStory == null) return false;

            _context.UserStories.Remove(userStory);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
