using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Models.ProjectsViewModels
{
    public class EditProjectViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int SprintDuration { get; set; }

        [Required]
        public List<string> UserIds { get; set; } = new();

        public List<SelectListItem> AllUsers { get; set; } = new();
        public List<SelectListItem> SprintDurations { get; set; } = new();
    }
}
