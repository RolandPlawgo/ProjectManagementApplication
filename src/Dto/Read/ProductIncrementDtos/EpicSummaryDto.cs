namespace ProjectManagementApplication.Dto.Read.ProductIncrementDtos
{
    public class EpicSummaryDto
    {
        public int Id { get; set; }
        public string Title = "";
        public List<UserStorySummaryDto> UserStories { get; set; } = new();
    }
}
