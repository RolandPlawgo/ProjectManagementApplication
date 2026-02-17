namespace ProjectManagementApplication.Dto.Read.ProductIncrementDtos
{
    public class SprintSummaryDto
    {
        public int Id { get; set; }
        public string SprintGoal = "";
        public List<UserStorySummaryDto> UserStories { get; set; } = new();
    }
}
