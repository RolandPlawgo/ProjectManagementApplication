using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Models.BacklogViewModels
{
    public class BacklogViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public List<EpicSummaryViewModel> Epics { get; set; } = new List<EpicSummaryViewModel>();
    }
}
