using Moq;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Requests.ScrumBoardRequests;
using ProjectManagementApplication.Services.Implementations;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication_UnitTests
{
    public class ScrumBoardServiceTests
    {
        [Fact]
        public async Task FinishSprintEarlyAsync_CorrectData_ReturnsSuccess()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.FinishSprintEarlyAsync(sprintId);

            Assert.Equal(ResultStatus.Success, result.Status);
        }
        [Fact]
        public async Task FinishSprintEarlyAsync_CorrectData_SetsSprintEndDate()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.FinishSprintEarlyAsync(sprintId);

            Assert.NotNull(context.Sprints.Find(sprintId)?.EndDate);
            Assert.True(context.Sprints.Find(sprintId)?.EndDate <= DateTime.Now);
        }

        [Fact]
        public async Task FinishSprintEarlyAsync_SprintNotActive_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" }); // Sprint not active
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false); // Sprint not active
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.FinishSprintEarlyAsync(sprintId);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
        [Fact]
        public async Task FinishSprintEarlyAsync_SprintAlreadyFinished_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" }); // Sprint already finished (past end date)
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true); // Sprint already finished
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.FinishSprintEarlyAsync(sprintId);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }

        [Fact]
        public async Task MoveCardAsync_CorrectData_ReturnsSuccess()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 1, Content = "", Title = "" } } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.inprogress, SubtaskId = 1, CurrentUserId = "" });

            Assert.Equal(ResultStatus.Success, result.Status);
        }
        [Fact]
        public async Task MoveCardAsync_MoveToTodo_SetsUserToNull()
        {
            int sprintId = 1;
            string userId = "id";
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 1, Content = "", Title = "", AssignedUser = new ApplicationUser() { Id = userId, Email = "", FirstName = "", LastName = "" } } } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.todo, SubtaskId = 1, CurrentUserId = userId });

            Assert.Null(context.UserStories.First().Subtasks.First().AssignedUser);
        }
        [Theory]
        [InlineData(TargetList.inprogress)]
        [InlineData(TargetList.done)]
        public async Task MoveCardAsync_MoveFromTodo_AssignsUser(TargetList targetList)
        {
            int sprintId = 1;
            string userId = "id";
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 1, Content = "", Title = "" } } }
            });
            context.Users.Add(new ApplicationUser() { Id = userId, Email = "", FirstName = "", LastName = "" });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveCardAsync(new MoveCardRequest() { TargetList = targetList, SubtaskId = 1, CurrentUserId = userId });

            Assert.NotNull(context.UserStories.First().Subtasks.First().AssignedUser);
            Assert.Equal(userId, context.UserStories.First().Subtasks.First().AssignedUserId);
        }
        [Fact]
        public async Task MoveCardAsync_MoveToDone_SetsDoneToTrue()
        {
            int sprintId = 1;
            string userId = "id";
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 1, Content = "", Title = "", Done = false, AssignedUser = new ApplicationUser() { Id = userId, Email = "", FirstName = "", LastName = "" } } } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.done, SubtaskId = 1, CurrentUserId = userId });

            Assert.True(context.UserStories.First().Subtasks.First().Done);
        }
        [Theory]
        [InlineData(TargetList.inprogress)]
        [InlineData(TargetList.todo)]
        public async Task MoveCardAsync_MoveFromDone_SetsDoneToFalse(TargetList targetList)
        {
            int sprintId = 1;
            string userId = "id";
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" });
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 1, Content = "", Title = "", Done = true, AssignedUser = new ApplicationUser() { Id = userId, Email = "", FirstName = "", LastName = "" } } } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveCardAsync(new MoveCardRequest() { TargetList = targetList, SubtaskId = 1, CurrentUserId = userId });

            Assert.False(context.UserStories.First().Subtasks.First().Done);
        }
        [Fact]
        public async Task MoveCardAsync_SprintNotActive_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = false, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" }); // Sprint not active
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 1, Content = "", Title = "" } } },
                new UserStory { Id = 2, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 2, Content = "", Title = "" } } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(false); // Sprint not active
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(false);
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.inprogress, SubtaskId = 1, CurrentUserId = "" });

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
        [Fact]
        public async Task MoveCardAsync_SprintAlreadyFinished_ReturnsValidationFailed()
        {
            int sprintId = 1;
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = sprintId, Active = true, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" }); // Sprint already finished (past date)
            context.UserStories.AddRange(new List<UserStory>
            {
                new UserStory { Id = 1, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 1, Content = "", Title = "" } } },
                new UserStory { Id = 2, SprintId = sprintId, Description = "", Title = "", Subtasks = new List<Subtask> { new Subtask { Id = 2, Content = "", Title = "" } } }
            });
            await context.SaveChangesAsync();
            var sprintServiceMock = new Mock<ISprintService>();
            sprintServiceMock.Setup(s => s.IsSprintActiveAsync(sprintId)).ReturnsAsync(true);
            sprintServiceMock.Setup(s => s.IsSprintFinishedAsync(sprintId)).ReturnsAsync(true); // Sprint already finished
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            var scrumBoardService = new ScrumBoardService(context, identityUserServiceMock.Object, sprintServiceMock.Object);

            var result = await scrumBoardService.MoveCardAsync(new MoveCardRequest() { TargetList = TargetList.inprogress, SubtaskId = 1, CurrentUserId = "" });

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        }
    }
}
