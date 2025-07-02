namespace ProjectManagementApplication.Models.SprintViewModels
{
    public class SprintReviewUserStorySummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string EpicTitle { get; set; } = "";
        public int CompletedTasksCount { get; set; }
        public int AllTasksCount { get; set; }
    }
}
