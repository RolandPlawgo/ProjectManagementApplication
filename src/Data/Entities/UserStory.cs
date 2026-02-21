using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Data.Entities
{
    public enum Status
    {
        Backlog = 0,
        Sprint = 1,
        ProductIncrement = 2
    }
    public class UserStory
    {
        public int Id { get; set; }
        [Required]
        [MinLength(1)]
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Status Status { get; set; }
        public int EpicId { get; set; }
        public Epic Epic { get; set; } = null!;
        public int? SprintId { get; set; }
        public Sprint? Sprint { get; set; } = null!;
        public List<Subtask> Subtasks { get; set; } = null!;
    }
}
