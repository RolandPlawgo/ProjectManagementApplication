namespace ProjectManagementApplication.Models.SprintViewModels
{
    public class ScrumBoardViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int SprintId { get; set; }
        public string SprintGoal { get; set; } = "";
        public int DaysToEndOfSprint { get; set; }
        public List<UserStorySummaryViewModel> UserStories { get; set; } = new();
        public List<ScrumBoardSubtaskSummaryViewModel> ToDoTasks { get; set; } = new();
        public List<ScrumBoardSubtaskSummaryViewModel> InProgressTasks { get; set; } = new();
        public List<ScrumBoardSubtaskSummaryViewModel> DoneTasks { get; set; } = new();
    }
}
