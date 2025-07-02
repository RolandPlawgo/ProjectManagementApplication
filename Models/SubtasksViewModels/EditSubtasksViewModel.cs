using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Models.Subtasks
{
    public class EditSubtaskViewModel
    {
        public int Id { get; set; }
        public int UserStoryId { get; set; }

        [Required]
        [Display(Name = "Subtask Title")]
        public string Title { get; set; } = null!;

        public string? Content { get; set; }
    }
}
