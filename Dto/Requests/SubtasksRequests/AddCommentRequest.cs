using ProjectManagementApplication.Authentication;

namespace ProjectManagementApplication.Dto.Requests.SubtasksRequests
{
    public class AddCommentRequest
    {
        public int SubtaskId { get; set; }
        public string Content { get; set; } = null!;
        public ApplicationUser Author { get; set; } = null!;
    }
}
