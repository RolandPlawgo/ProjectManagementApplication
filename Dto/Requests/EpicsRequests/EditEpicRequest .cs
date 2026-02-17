using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Dto.Requests.EpicsViewModels
{
    public class EditEpicRequest
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = "";
    }
}
