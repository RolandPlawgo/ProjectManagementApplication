namespace ProjectManagementApplication.Models.ProjectsViewModels
{
    public class UserWithRolesViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
