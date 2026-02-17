using ProjectManagementApplication.Dto.Requests.ScrumBoardRequests;

namespace ProjectManagementApplication.Dto.Requests.SprintReviewRequests
{
    public enum TargetList
    {
        ProductBacklog,
        SprintBacklog,
        ProductIncrement
    }
    public class MoveCardRequest
    {
        public int UserStoryId { get; set; }
        public TargetList TargetList { get; set; }
    }
}
