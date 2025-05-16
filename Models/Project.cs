using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Models
{
	public class Project
	{
		public int Id { get; set; }
        [Required]
        [MinLength(1)]
        public List<string> UserId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int SprintDuration { get; set; }
    }
}
