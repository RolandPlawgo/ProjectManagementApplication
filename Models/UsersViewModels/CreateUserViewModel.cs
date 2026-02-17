using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectManagementApplication.Models.UsersViewModels
{
	public class CreateUserViewModel
	{
		public string Email { get; set; } = "";
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string Role { get; set; } = "";
		public List<string> RolesOptions { get; set; } = new List<string>();
	}
}
