using Moq;
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Requests.SprintPlanningRequests;
using ProjectManagementApplication.Services.Implementations;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication_UnitTests
{
    public class SprintPlanningServiceTests
    {
        [Theory]
        [InlineData(Status.Backlog, Status.Sprint)]
        [InlineData(Status.Sprint, Status.Backlog)]
        public async Task MoveUserStory_CorrectData_ReturnsSuccess(Status currentStatus, Status targetStatus)
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = currentStatus, Description = "", Title = "", Epic = new Epic() { Id = 1, Title = "" } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveUserStory(new MoveUserStoryRequest() { TargetStatus = targetStatus, UserStoryId = 1, SprintId = sprintId });

            Assert.Equal(ResultStatus.Success, result.Status);
        }
        [Theory]
        [InlineData(Status.Backlog, Status.Sprint)]
        [InlineData(Status.Sprint, Status.Backlog)]
        public async Task MoveUserStory_CorrectData_UpdatesStatus(Status currentStatus, Status targetStatus)
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = currentStatus, Description = "", Title = "", Epic = new Epic() { Id = 1, Title = "" } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveUserStory(new MoveUserStoryRequest() { TargetStatus = targetStatus, UserStoryId = 1, SprintId = sprintId });

            Assert.Equal(targetStatus, context.UserStories.First().Status);
        }
        [Fact]
        public async Task MoveUserStory_MoveToSprintColumn_AssignsSprint()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = null, Status = Status.Backlog, Description = "", Title = "", Epic = new Epic() { Id = 1, Title = "" } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveUserStory(new MoveUserStoryRequest() { TargetStatus = Status.Sprint, UserStoryId = 1, SprintId = sprintId });

            Assert.Equal(sprintId, context.UserStories.First().SprintId);
        }
        [Fact]
        public async Task MoveUserStory_MoveToBacklogColumn_SetsSprintToNull()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Sprint, Description = "", Title = "", Epic = new Epic() { Id = 1, Title = "" } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveUserStory(new MoveUserStoryRequest() { TargetStatus = Status.Backlog, UserStoryId = 1, SprintId = sprintId });

            Assert.Null(context.UserStories.First().Sprint);
        }
        [Fact]
        public async Task MoveUserStory_SprintIsActive_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, SprintGoal = "" }); // Sprint is active
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Status = Status.Backlog, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 1, Content = "", Title = "" } } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true); // Sprint is active
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveUserStory(new MoveUserStoryRequest() { TargetStatus = Status.Sprint, UserStoryId = 1, SprintId = sprintId });

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }

        [Fact]
        public async Task SetSprintGoalAsync_CorrectData_ReturnsSuccess()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "" });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.SetSprintGoalAsync(sprintId, "");

            Assert.Equal(ResultStatus.Success, result.Status);
        }
        [Fact]
        public async Task SetSprintGoalAsync_CorrectData_SetsSprintGoal()
        {
            int sprintId = 1;
            string sprintGoal = "sprint goal";
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "" });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.SetSprintGoalAsync(sprintId, sprintGoal);

            Assert.Equal(sprintGoal, context.Sprints.First().SprintGoal);
        }
        [Fact]
        public async Task SetSprintGoalAsync_SprintIsActive_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, SprintGoal = "" }); // Sprint is active
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true); // Sprint is active
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.SetSprintGoalAsync(sprintId, "");

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }

        [Fact]
        public async Task StartSprint_CorrectData_ReturnsSuccess()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "", Project = new Project { Id = 1, Name = "", Description = "", SprintDuration = 0 } }); 
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.StartSprint(sprintId);

            Assert.Equal(ResultStatus.Success, result.Status);
        }
        [Fact]
        public async Task StartSprint_CorrectData_SetsActiveToTrue()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "", Project = new Project { Id = 1, Name = "", Description = "", SprintDuration = 0 } });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.StartSprint(sprintId);

            Assert.True(context.Sprints.First().Active);
        }
        [Fact]
        public async Task StartSprint_CorrectData_SetsStartDate()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "", Project = new Project { Id = 1, Name = "", Description = "", SprintDuration = 0 } });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.StartSprint(sprintId);

            Assert.NotNull(context.Sprints.First().StartDate);
            Assert.True(context.Sprints.First().StartDate <= DateTime.Now);
        }
        [Fact]
        public async Task StartSprint_CorrectData_SetsEndDate()
        {
            int sprintId = 1;
            int sprintDuration = 2;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, SprintGoal = "", Project = new Project { Id = 1, Name = "", Description = "", SprintDuration = sprintDuration } });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false);
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.StartSprint(sprintId);

            Assert.NotNull(context.Sprints.First().EndDate);
            Assert.True(context.Sprints.First().EndDate <= DateTime.Now.AddDays(sprintDuration * 7));
        }
        [Fact]
        public async Task StartSprint_SprintAlreadyActive_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, SprintGoal = "", Project = new Project { Id = 1, Name = "", Description = "", SprintDuration = 0 } }); // Sprint is already active
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true); // Sprint is already active
            var scrumBoardService = new SprintPlanningService(context, sprintServiceMock.Object);

            var result = await scrumBoardService.StartSprint(sprintId);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
    }
}
