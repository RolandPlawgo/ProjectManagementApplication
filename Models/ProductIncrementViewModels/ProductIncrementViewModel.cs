namespace ProjectManagementApplication.Models.ProductIncrementViewModels
{
    public class ProductIncrementViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public List<SprintSummaryViewModel> Sprints { get; set; } = new();
        public List<EpicSummaryViewModel> Epics { get; set; } = new();
    }
}
