namespace ProjectManagementApplication.Dto.Read.ProjectsDtos
{
    public class ProjectDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SprintDuration { get; set; }
        public List<UserWithRolesDto> UsersWithRoles { get; set; } = new();
    }
}
