namespace ProjectManagementApplication.Dto.Read.MeetingsDtos
{
    public class ProjectSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<MeetingSummaryDto> Meetings { get; set; } = new();
    }
}