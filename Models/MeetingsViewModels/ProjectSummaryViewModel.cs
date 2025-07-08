namespace ProjectManagementApplication.Models.MeetingsViewModels
{
    public class ProjectSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<MeetingSummaryViewModel> Meetings { get; set; } = new();
    }
}