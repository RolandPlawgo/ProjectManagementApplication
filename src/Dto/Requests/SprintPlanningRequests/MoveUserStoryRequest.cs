using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Dto.Requests.SprintPlanningRequests
{
    public class MoveUserStoryRequest
    {
        public int UserStoryId { get; set; }
        public Status TargetStatus { get; set; }
        public int SprintId { get; set; }
    }
}
