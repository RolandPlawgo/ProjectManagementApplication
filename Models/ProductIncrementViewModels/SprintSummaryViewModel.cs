namespace ProjectManagementApplication.Models.ProductIncrementViewModels
{
    public class SprintSummaryViewModel
    {
        public int Id { get; set; }
        public string SprintGoal = "";
        public List<UserStorySummaryViewModel> UserStories { get; set; } = new();
    }
}
