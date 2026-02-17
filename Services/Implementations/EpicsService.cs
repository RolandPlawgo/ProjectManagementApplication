using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.EpicsDtos;
using ProjectManagementApplication.Dto.Requests.EpicsViewModels;
using ProjectManagementApplication.Models.EpicViewModels;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Services.Implementations
{
    public class EpicsService : IEpicsService
    {
        private readonly ApplicationDbContext _context;
        public EpicsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateEpicAsync(CreateEpicRequest createEpicRequest)
        {
            var epic = new Epic
            {
                Title = createEpicRequest.Title,
                ProjectId = createEpicRequest.ProjectId
            };
            _context.Epics.Add(epic);
            await _context.SaveChangesAsync();
        }

        public async Task<EditEpicDto?> GetEpicForEditAsync(int id)
        {
            var epic = await _context.Epics.FindAsync(id);
            if (epic == null) return null;

            var epicDto = new EditEpicDto
            {
                Id = epic.Id,
                Title = epic.Title,
                ProjectId = epic.ProjectId
            };
            return epicDto;
        }

        public async Task<bool> EditEpicAsync(EditEpicRequest editEpicRequest)
        {
            var epic = await _context.Epics.FindAsync(editEpicRequest.Id);
            if (epic == null) return false;

            epic.Title = editEpicRequest.Title;
            _context.Epics.Update(epic);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteEpicAsync(int id)
        {
            var epic = await _context.Epics
                .Include(e => e.UserStories)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (epic == null) return false;

            _context.UserStories.RemoveRange(epic.UserStories);
            _context.Epics.Remove(epic);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int?> GetProjectIdForEpicAsync(int epicId)
        {
            Epic? epic = await _context.Epics.FindAsync(epicId);
            if (epic == null) return null;
            return epic.ProjectId;
        }
    }
}
