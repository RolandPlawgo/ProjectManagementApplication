using ProjectManagementApplication.Authentication;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Data.Entities
{
    public class Subtask
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int UserStoryId { get; set; }
        public UserStory UserStory { get; set; } = null!;
        public List<Comment> Comments { get; set; } = null!;
        public string? AssignedUserId { get; set; }
        public ApplicationUser? AssignedUser { get; set; }
        public bool Done { get; set; }
    }
}
