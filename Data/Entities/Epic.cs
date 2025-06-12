using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Data.Entities
{
    public class Epic
    {
        public int Id { get; set; }
        [Required]
        [MinLength(1)]
        public string Title { get; set; } = null!;
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public List<UserStory> UserStories { get; set; } = new List<UserStory>();
    }
}
