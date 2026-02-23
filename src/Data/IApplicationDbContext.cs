using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Data
{
    public interface IApplicationDbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Epic> Epics { get; set; }
        public DbSet<UserStory> UserStories { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Subtask> Subtasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
