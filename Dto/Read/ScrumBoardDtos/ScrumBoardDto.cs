namespace ProjectManagementApplication.Dto.Read.ScrumBoardDtos
{
    public class ScrumBoardDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int SprintId { get; set; }
        public string SprintGoal { get; set; } = "";
        public int DaysToEndOfSprint { get; set; }
        public List<UserStorySummaryDto> UserStories { get; set; } = new();
        public List<SubtaskSummaryDto> ToDoTasks { get; set; } = new();
        public List<SubtaskSummaryDto> InProgressTasks { get; set; } = new();
        public List<SubtaskSummaryDto> DoneTasks { get; set; } = new();
    }
}
