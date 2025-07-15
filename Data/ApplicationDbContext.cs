using ProjectManagementApplication.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Epic> Epics { get; set; }
        public DbSet<UserStory> UserStories { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Subtask> Subtasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<UserStory>()
               .HasOne(us => us.Sprint)
               .WithMany(s => s.UserStories)
               .HasForeignKey(us => us.SprintId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
