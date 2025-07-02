namespace ProjectManagementApplication.Models.SprintViewModels
{
    public class SprintReviewViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int SprintId { get; set; }
        public string SprintGoal { get; set; } = "";
        public List<SprintReviewUserStorySummaryViewModel> ProductBacklogUserStories { get; set; } = new();
        public List<SprintReviewUserStorySummaryViewModel> SprintBacklogUserStories { get; set; } = new();
        public List<SprintReviewUserStorySummaryViewModel> ProductIncrementUserStories { get; set; } = new();
    }
}
