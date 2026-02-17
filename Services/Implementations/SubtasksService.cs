using Humanizer;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.SubtasksDtos;
using ProjectManagementApplication.Dto.Requests.SubtasksRequests;
using ProjectManagementApplication.Helpers;
using ProjectManagementApplication.Services.Interfaces;
using System.Threading.Tasks;

namespace ProjectManagementApplication.Services.Implementations
{
    public class SubtasksService : ISubtasksService
    {
        private readonly ApplicationDbContext _context;
        public SubtasksService(ApplicationDbContext context) 
        {
            _context = context;
        }
        public async Task CreateSubtaskAsync(CreateSubtaskRequest request)
        {
            var subtask = new Subtask
            {
                Title = request.Title,
                Content = request.Content ?? "",
                UserStoryId = request.UserStoryId,
                Done = false
            };
            _context.Subtasks.Add(subtask);
            await _context.SaveChangesAsync();
        }
        public async Task<SubtaskDetailsDto?> GetDetailsAsync(int id)
        {
            var subtask = await _context.Subtasks
                .Include(s => s.Comments)
                .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (subtask == null) return null;

            var dto = new SubtaskDetailsDto
            {
                Id = subtask.Id,
                Title = subtask.Title,
                Content = subtask.Content,
                Comments = subtask.Comments
                       .OrderBy(c => c.CreatedAt)
                    .Select(c => new CommentDto
                    {
                        Content = c.Content,
                        AuthorInitials = ApplicationUserHelper.UserInitials(c.Author),
                        CreatedAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                    }).ToList()
            };
            return dto;
        }

        public async Task<EditSubtaskDto?> GetForEditAsync(int id)
        {
            Subtask? subtask = await _context.Subtasks.FindAsync(id);
            if (subtask == null) return null;
            var dto = new EditSubtaskDto
            {
                Id = subtask.Id,
                Title = subtask.Title,
                Content = subtask.Content,
                UserStoryId = subtask.UserStoryId
            };
            return dto;
        }

        public async Task<bool> EditSubtaskAsync(EditSubtaskRequest request)
        {
            var subtask = await _context.Subtasks.FirstOrDefaultAsync(s => s.Id == request.Id);
            if (subtask == null) return false;

            subtask.Title = request.Title;
            subtask.Content = request.Content ?? "";
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSubtaskAsync(int id)
        {
            var subtask = await _context.Subtasks.FirstOrDefaultAsync(s => s.Id == id);
            if (subtask == null) return false;

            _context.Subtasks.Remove(subtask);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task AddCommentAsync(AddCommentRequest request)
        {
            var comment = new Comment
            {
                TaskId = request.SubtaskId,
                Content = request.Content,
                Author = request.Author,
                CreatedAt = DateTime.Now
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }
    }
}
