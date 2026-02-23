namespace ProjectManagementApplication.Dto.Read.SprintPlanningDtos
{
    public class SubtaskSummaryDto
    {
        public int Id { get; set; }
        public int UserStoryId { get; set; }
        public string Title { get; set; } = "";
    }
}