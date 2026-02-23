using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Dto.Read.EpicsDtos
{
    public class EditEpicDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = "";
    }
}
