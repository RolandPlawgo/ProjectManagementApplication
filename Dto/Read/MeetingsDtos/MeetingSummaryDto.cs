using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Dto.Read.MeetingsDtos
{
    public class MeetingSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; } = "";
        public string Time { get; set; } = "";
        public string TypeOfMeeting { get; set; } = "";
    }
}