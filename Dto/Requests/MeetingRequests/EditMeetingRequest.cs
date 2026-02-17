using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Dto.Requests.MeetingRequests
{
    public class EditMeetingRequest
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime Time { get; set; }
        public TypeOfMeeting TypeOfMeeting { get; set; }
    }
}
