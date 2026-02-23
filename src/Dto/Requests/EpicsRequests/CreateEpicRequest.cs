using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Dto.Requests.EpicsViewModels
{
    public class CreateEpicRequest
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = "";
    }
}
