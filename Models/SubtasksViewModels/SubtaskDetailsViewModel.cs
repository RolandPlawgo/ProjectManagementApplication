namespace ProjectManagementApplication.Models.SubtasksViewModels
{
    public class SubtaskDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public List<CommentViewModel> Comments { get; set; } = new();
    }
}
