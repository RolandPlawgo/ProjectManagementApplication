namespace ProjectManagementApplication.Dto.Read.SprintReviewDtos
{
    public class UserStoryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string EpicTitle { get; set; } = "";
        public int CompletedTasksCount { get; set; }
        public int AllTasksCount { get; set; }
    }
}
