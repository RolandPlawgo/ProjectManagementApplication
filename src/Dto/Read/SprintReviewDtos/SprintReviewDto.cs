namespace ProjectManagementApplication.Dto.Read.SprintReviewDtos
{
    public class SprintReviewDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int SprintId { get; set; }
        public string SprintGoal { get; set; } = "";
        public List<UserStoryDto> ProductBacklogUserStories { get; set; } = new();
        public List<UserStoryDto> SprintBacklogUserStories { get; set; } = new();
        public List<UserStoryDto> ProductIncrementUserStories { get; set; } = new();
    }
}
