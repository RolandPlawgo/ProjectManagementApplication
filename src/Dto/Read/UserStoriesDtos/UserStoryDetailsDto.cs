namespace ProjectManagementApplication.Dto.Read.UserStoriesDtos
{
    public class UserStoryDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string EpicTitle { get; set; } = "";
    }
}
