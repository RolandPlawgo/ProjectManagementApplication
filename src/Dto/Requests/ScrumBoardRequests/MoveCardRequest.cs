namespace ProjectManagementApplication.Dto.Requests.ScrumBoardRequests
{
    public enum TargetList
    {
        todo,
        inprogress,
        done
    }
    public class MoveCardRequest
    {
        public int SubtaskId { get; set; }
        public TargetList TargetList { get; set; }
        public string CurrentUserId { get; set; } = String.Empty;
    }
}
