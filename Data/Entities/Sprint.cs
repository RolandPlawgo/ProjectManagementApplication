namespace ProjectManagementApplication.Data.Entities
{
    public class Sprint
    {
        public int Id { get; set; }
        public string SprintGoal { get; set; } = null!;
        public bool Active { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public List<UserStory> UserStories { get; set; } = new List<UserStory>();
    }
}
