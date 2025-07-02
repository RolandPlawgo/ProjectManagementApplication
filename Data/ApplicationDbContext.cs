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
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Projects
            builder.Entity<Project>().HasData(new Project
            {
                Id = 100,
                Name = "Sample Project",
                Description = "Sample project added by deafult",
                SprintDuration = 2
            });

            // Seed Epics
            builder.Entity<Epic>().HasData(
                new Epic { Id = 1, ProjectId = 100, Title = "Epic: Authentication" },
                new Epic { Id = 2, ProjectId = 100, Title = "Epic: Authorization" }
            );

            // Seed UserStories
            builder.Entity<UserStory>().HasData(
                new UserStory { Id = 1, EpicId = 1, Title = "Login page", Description = "Allow users to log in", Status = Status.Backlog },
                new UserStory { Id = 2, EpicId = 1, Title = "Registration", Description = "Allow user registration", Status = Status.Backlog },
                new UserStory { Id = 3, EpicId = 2, Title = "Role management", Description = "CRUD roles", Status = Status.Backlog },
                new UserStory { Id = 4, EpicId = 2, Title = "Claim-based auth", Description = "Implement claim checks", Status = Status.Backlog }
            );


            // Sprint -> UserStories: NO cascade
            builder.Entity<UserStory>()
               .HasOne(us => us.Sprint)
               .WithMany(s => s.UserStories)
               .HasForeignKey(us => us.SprintId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
