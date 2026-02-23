namespace ProjectManagementApplication.Dto.Read.ScrumBoardDtos
{
    public class SubtaskSummaryDto
    {
        public int Id { get; set; }
        public int UserStoryId { get; set; }
        public string Title { get; set; } = "";
        public int CommentsCount { get; set; }
        public string? AssignedUserInitials { get; set; }
    }
}
