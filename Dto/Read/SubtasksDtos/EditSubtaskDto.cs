using System.ComponentModel.DataAnnotations;

namespace ProjectManagementApplication.Dto.Read.SubtasksDtos
{
    public class EditSubtaskDto
    {
        public int Id { get; set; }
        public int UserStoryId { get; set; }
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
    }
}
