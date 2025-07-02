using ProjectManagementApplication.Authentication;

namespace ProjectManagementApplication.Data.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public int TaskId { get; set; }
        public Subtask Task { get; set; } = null!;
        public string AuthorId { get; set; } = null!;
        public ApplicationUser Author { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
