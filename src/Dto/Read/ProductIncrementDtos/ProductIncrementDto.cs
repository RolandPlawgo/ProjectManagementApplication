namespace ProjectManagementApplication.Dto.Read.ProductIncrementDtos
{
    public class ProductIncrementDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public List<SprintSummaryDto> Sprints { get; set; } = new();
        public List<EpicSummaryDto> Epics { get; set; } = new();
    }
}
