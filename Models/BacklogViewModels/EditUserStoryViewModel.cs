using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Models.BacklogViewModels
{
    public class EditUserStoryViewModel
    {
        public int Id { get; set; }

        [Required]
        public int EpicId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        [Required, StringLength(500)]
        public string Description { get; set; } = "";

        public IEnumerable<SelectListItem> Epics { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
