namespace ProjectManagementApplication.Models.ProductIncrementViewModels
{
    public class EpicSummaryViewModel
    {
        public int Id { get; set; }
        public string Title = "";
        public List<UserStorySummaryViewModel> UserStories { get; set; } = new();
    }
}