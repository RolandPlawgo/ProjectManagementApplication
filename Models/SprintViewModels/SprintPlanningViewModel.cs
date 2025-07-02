namespace ProjectManagementApplication.Models.SprintViewModels
{
    public class SprintPlanningViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int SprintId { get; set; }
        public string SprintGoal { get; set; } = "";
        public List<UserStorySummaryViewModel> BacklogUserStories { get; set; } = new();
        public List<UserStorySummaryViewModel> SprintUserStories { get; set; } = new();
        public List<SubtaskSummaryViewModel> Subtasks { get; set; } = new();
    }
}
