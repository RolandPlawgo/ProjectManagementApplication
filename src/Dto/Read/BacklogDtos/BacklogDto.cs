using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Dto.Read.BacklogDtos
{
    public class BacklogDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public List<EpicSummaryDto> Epics { get; set; } = new List<EpicSummaryDto>();
    }
}
