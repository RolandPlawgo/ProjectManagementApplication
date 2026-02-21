using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Models.EpicViewModels
{
    public class CreateEpicViewModel
    {
        [Required]
        public int ProjectId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = "";
    }
}
