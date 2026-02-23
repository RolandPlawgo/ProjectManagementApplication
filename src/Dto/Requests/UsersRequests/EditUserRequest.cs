namespace ProjectManagementApplication.Dto.Requests.UsersRequests
{
    public class EditUserRequest
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
