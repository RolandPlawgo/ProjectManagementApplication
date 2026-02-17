using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Dto.Requests.UserStoriesRequests
{
    public class CreateUserStoryRequest
    {
        public int Id { get; set; }
        public int EpicId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
