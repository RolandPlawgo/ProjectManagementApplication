using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Models.UserStoryViewModels
{
    public class CreateUserStoryViewModel
    {
        public int Id { get; set; }
        [Required]
        public int EpicId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        [Required, StringLength(500)]
        public string Description { get; set; } = "";
    }
}
