namespace ProjectManagementApplication.Dto.Read.SprintPlanningDtos
{
    public class SprintPlanningDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public int SprintId { get; set; }
        public string SprintGoal { get; set; } = "";
        public List<UserStorySummaryDto> BacklogUserStories { get; set; } = new();
        public List<UserStorySummaryDto> SprintUserStories { get; set; } = new();
        public List<SubtaskSummaryDto> Subtasks { get; set; } = new();
    }
}
