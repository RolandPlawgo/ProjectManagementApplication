using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Models.BacklogViewModels
{
    public class EpicViewModel
    {
        public int Id { get; set; }
        [Required]
        public int ProjectId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = "";
    }
}
