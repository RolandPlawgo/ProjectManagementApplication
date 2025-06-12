using Microsoft.AspNetCore.Identity;
using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Authentication
{
	public class ApplicationUser : IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool MustChangePassword { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
