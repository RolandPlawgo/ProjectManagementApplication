using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Models.EpicViewModels
{
    public class EditEpicViewModel
    {
        public int Id { get; set; }
        [Required]
        public int ProjectId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = "";
    }
}
