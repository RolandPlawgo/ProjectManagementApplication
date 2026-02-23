using Microsoft.EntityFrameworkCore;
using Moq;
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Requests.MeetingRequests;
using ProjectManagementApplication.Services.Implementations;

namespace ProjectManagementApplication_UntTests
{
    public class MeetingsServiceTests
    {
        [Fact]
        public async Task CreateMeetingAsync_PastDate_ReturnsValidationFailed()
        {
            var dbContextSub = new Mock<IApplicationDbContext>();
            MeetingsService service = new MeetingsService(dbContextSub.Object);

            var result = await service.CreateMeetingAsync(new CreateMeetingRequest()
            {
                Name = "Test Meeting",
                Description = "",
                Time = DateTime.Now.AddDays(-1), // Past date
                TypeOfMeeting = TypeOfMeeting.DailyScrum,
                ProjectId = 1
            });

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
    }
}