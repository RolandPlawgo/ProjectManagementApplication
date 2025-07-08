namespace ProjectManagementApplication.Data.Entities
{
    public enum TypeOfMeeting
    {
        SprintPlanning = 0,
        DailyScrum = 1,
        SprintReview = 2,
        SprintRetrospective = 3
    }

    public class Meeting
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime Time { get; set; }
        public TypeOfMeeting TypeOfMeeting { get; set; }
    }
}
