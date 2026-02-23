using Moq;
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Requests.SprintReviewRequests;
using ProjectManagementApplication.Services.Implementations;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication_UnitTests
{
    public class SprintReviewServiceTests
    {
        [Fact]
        public async Task FinishSprintAsync_CorrectData_ReturnsSuccess()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Backlog, Description = "", Title = "" }, 
                new UserStory { Id = 2, SprintId = sprintId, Status = Status.ProductIncrement, Description = "", Title = "" }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.FinishSprintAsync(sprintId);

            Assert.Equal(ResultStatus.Success, result.Status);
        }
        [Fact]
        public async Task FinishSprintAsync_CorrectData_SetsSprintActiveToFalse()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Backlog, Description = "", Title = "" },
                new UserStory { Id = 2, SprintId = sprintId, Status = Status.ProductIncrement, Description = "", Title = "" }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            await sprintReviewService.FinishSprintAsync(sprintId);

            Assert.False(context.Sprints.Find(1)?.Active);
        }

        [Fact]
        public async Task FinishSprintAsync_NotAllTasksAcceptedOrRejected_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Sprint, Description = "", Title = "" }, // This story is still in the sprint, not accepted or rejected
                new UserStory { Id = 2, SprintId = sprintId, Status = Status.ProductIncrement, Description = "", Title = "" }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.FinishSprintAsync(sprintId);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
        [Fact]
        public async Task FinishSprintAsync_SprintNotActive_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" }); // Sprint not active
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Backlog, Description = "", Title = "" },
                new UserStory { Id = 2, SprintId = sprintId, Status = Status.ProductIncrement, Description = "", Title = "" }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false); // Sprint not active
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.FinishSprintAsync(sprintId);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
        [Fact]
        public async Task FinishSprintAsync_SprintNotFinished_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" }); // Sprint not finished (future end date)
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Backlog, Description = "", Title = "" },
                new UserStory { Id = 2, SprintId = sprintId, Status = Status.ProductIncrement, Description = "", Title = "" }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false); // Sprint not finished
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.FinishSprintAsync(sprintId);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }

        [Theory]
        [InlineData(Status.Sprint, TargetList.ProductBacklog)]
        [InlineData(Status.Sprint, TargetList.ProductIncrement)]
        [InlineData(Status.Backlog, TargetList.SprintBacklog)]
        [InlineData(Status.Backlog, TargetList.ProductIncrement)]
        [InlineData(Status.ProductIncrement, TargetList.SprintBacklog)]
        [InlineData(Status.ProductIncrement, TargetList.ProductBacklog)]
        public async Task MoveCardAsync_ProperData_ReturnsSuccess(Status currentStatus, TargetList targetList)
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = currentStatus, Description = "", Title = "" },
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.MoveCardAsync(new MoveCardRequest() { TargetList = targetList, UserStoryId = 1 });

            Assert.Equal(ResultStatus.Success, result.Status);
        }
        [Theory]
        [InlineData(Status.Sprint, TargetList.ProductBacklog, Status.Backlog)]
        [InlineData(Status.Sprint, TargetList.ProductIncrement, Status.ProductIncrement)]
        [InlineData(Status.Backlog, TargetList.SprintBacklog, Status.Sprint)]
        [InlineData(Status.Backlog, TargetList.ProductIncrement, Status.ProductIncrement)]
        [InlineData(Status.ProductIncrement, TargetList.SprintBacklog, Status.Sprint)]
        [InlineData(Status.ProductIncrement, TargetList.ProductBacklog, Status.Backlog)]
        public async Task MoveCardAsync_ProperData_UpdatesUserStoryStatus(Status currentStatus, TargetList targetList, Status targetStatus)
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = currentStatus, Description = "", Title = "" },
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.MoveCardAsync(new MoveCardRequest() { TargetList = targetList, UserStoryId = 1 });

            Assert.Equal(targetStatus, context.UserStories.Find(1)?.Status);
        }
        [Fact]
        public async Task MoveCardAsync_SprintNotActive_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" }); // Sprint not active
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Backlog, Description = "", Title = "" },
                new UserStory { Id = 2, SprintId = sprintId, Status = Status.ProductIncrement, Description = "", Title = "" }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false); // Sprint not active
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.ProductBacklog, UserStoryId = 1});

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
        [Fact]
        public async Task MoveCardAsync_SprintNotFinished_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" }); // Sprint not finished (future end date)
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Sprint, Description = "", Title = "" }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false); // Sprint not finished
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.ProductIncrement, UserStoryId = 1 });

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
        [Fact]
        public async Task MoveCardAsync_CardAlreadyInIncrement_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.ProductIncrement, Description = "", Title = "" },
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.ProductIncrement, UserStoryId = 1 });

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
        [Fact]
        public async Task MoveCardAsync_CardAlreadyInProductBacklog_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Backlog, Description = "", Title = "" },
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.ProductBacklog, UserStoryId = 1 });

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
        [Fact]
        public async Task MoveCardAsync_CardAlreadyInSprintBacklog_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Sprint, Description = "", Title = "" },
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true);
            var sprintReviewService = new SprintReviewService(context, sprintServiceMock.Object);

            var result = await sprintReviewService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.SprintBacklog, UserStoryId = 1 });

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
    }
}
