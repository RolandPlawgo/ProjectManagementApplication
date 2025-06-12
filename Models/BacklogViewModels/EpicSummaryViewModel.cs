namespace ProjectManagementApplication.Models.BacklogViewModels
{
    public class EpicSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public List<UserStorySummaryViewModel> UserStories { get; set; } = new();
    }
}
