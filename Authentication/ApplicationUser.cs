using Microsoft.AspNetCore.Identity;
using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Authentication
{
	public class ApplicationUser : IdentityUser
	{
		public string FirstName { get; set; } = null!;
		public string LastName { get; set; } = null!;
		public bool MustChangePassword { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();
		public ICollection<Subtask> Subtasks { get; set; } = new List<Subtask>();
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
