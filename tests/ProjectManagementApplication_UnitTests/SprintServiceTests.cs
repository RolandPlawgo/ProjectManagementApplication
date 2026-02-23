using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Services.Implementations;

namespace ProjectManagementApplication_UnitTests
{
    public class SprintServiceTests
    {
        [Fact]
        public async Task IsSprintActiveAsync_NotActive_ReturnsFalse()
        {
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = 1, Active = false, SprintGoal = "" });
            var sprintService = new SprintService(context);

            bool? result = await sprintService.IsSprintActiveAsync(1);

            Assert.False(result);
        }
        [Fact]
        public async Task IsSprintActiveAsync_Active_ReturnsTrue()
        {
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = 1, Active = true, SprintGoal = "" });
            var sprintService = new SprintService(context);

            bool? result = await sprintService.IsSprintActiveAsync(1);

            Assert.True(result);
        }
        [Fact]
        public async Task IsSprintFinishedAsync_FutureDate_ReturnsFalse()
        {
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = 1, EndDate = DateTime.Now.AddDays(1), SprintGoal = "" });
            var sprintService = new SprintService(context);

            bool? result = await sprintService.IsSprintFinishedAsync(1);

            Assert.False(result);
        }
        [Fact]
        public async Task IsSprintFinishedAsync_PastDate_ReturnsTrue()
        {
            using var context = DbContextHelper.CreateDbContext();
            context.Sprints.Add(new Sprint { Id = 1, EndDate = DateTime.Now.AddDays(-1), SprintGoal = "" });
            var sprintService = new SprintService(context);

            bool? result = await sprintService.IsSprintFinishedAsync(1);

            Assert.True(result);
        }
    }
}
