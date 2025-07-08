using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication.Models.MeetingsViewModels
{
    public class CreateMeetingViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime Time { get; set; }
        public TypeOfMeeting TypeOfMeeting { get; set; }
        public IEnumerable<SelectListItem> TypeOfMeetingOptions { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
