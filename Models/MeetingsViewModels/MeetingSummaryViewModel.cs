using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Models.MeetingsViewModels
{
    public class MeetingSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; } = "";
        public string Time { get; set; } = "";
        public string TypeOfMeeting { get; set; } = "";
    }
}