namespace ProjectManagementApplication.Dto.Read.ProjectsDtos
{
    public class ProjectCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> UserInitials { get; set; } = new();
    }
}
