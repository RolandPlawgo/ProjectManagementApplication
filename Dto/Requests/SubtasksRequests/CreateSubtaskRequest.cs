using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Dto.Requests.SubtasksRequests
{
    public class CreateSubtaskRequest
    {
        public int UserStoryId { get; set; }
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
    }
}
