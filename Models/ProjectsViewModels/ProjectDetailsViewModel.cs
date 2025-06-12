namespace ProjectManagementApplication.Models.ProjectsViewModels
{
    //public class ProjectsIndexViewModel
    //{
    //    public List<ProjectCardViewModel> Projects { get; set; } = new List<ProjectCardViewModel>();
    //}

    public class ProjectDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SprintDuration { get; set; }
        public List<UserWithRolesViewModel> UsersWithRoles { get; set; } = new();
    }
}
