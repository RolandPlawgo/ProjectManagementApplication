namespace ProjectManagementApplication.Dto.Read.BacklogDtos
{
    public class EpicSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<UserStorySummaryDto> UserStories { get; set; } = new();
    }
}
