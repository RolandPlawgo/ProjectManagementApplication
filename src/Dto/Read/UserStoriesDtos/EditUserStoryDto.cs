using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Dto.Read.UserStoriesDtos
{
    public class EditUserStoryDto
    {
        public int Id { get; set; }
        public int EpicId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<EpicSummaryDto> Epics { get; set; } = new();
    }
}
