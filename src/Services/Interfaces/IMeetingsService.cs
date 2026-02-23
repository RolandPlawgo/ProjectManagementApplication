using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Dto.Read.MeetingsDtos;
using ProjectManagementApplication.Dto.Requests.MeetingRequests;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IMeetingsService
    {
        public Task<MeetingsDto> GetMeetingsAsync(ApplicationUser user);
        public Task<Result> CreateMeetingAsync(CreateMeetingRequest request);
        public Task<EditMeetingDto?> GetForEditAsync(int id);
        public Task<Result> EditMeetingAsync(EditMeetingRequest request);
        public Task<bool> DeleteMeetingAsync(int id);
    }
}
