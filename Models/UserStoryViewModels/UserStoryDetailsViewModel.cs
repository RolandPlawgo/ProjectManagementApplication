namespace ProjectManagementApplication.Models.UserStoryViewModels
{
    public class UserStoryDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string EpicTitle { get; set; } = "";
    }
}
