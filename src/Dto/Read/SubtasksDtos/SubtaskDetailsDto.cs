namespace ProjectManagementApplication.Dto.Read.SubtasksDtos
{
    public class SubtaskDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public List<CommentDto> Comments { get; set; } = new();
    }
}
