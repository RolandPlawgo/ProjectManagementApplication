using ProjectManagementApplication.Authentication;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Data.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        [Required]
        [MinLength(1)]
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        /// <summary>
        /// Sprint length in weeks
        /// </summary>
        [Required]
        public int SprintDuration { get; set; }
        public List<Epic> Epics { get; set; } = new List<Epic>();
        public List<Sprint> Sprints { get; set; } = new List<Sprint>();
        public List<Meeting> Meetings { get; set; } = new List<Meeting>();
    }
}
