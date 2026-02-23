using Moq;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Requests.ProjectsRequests;
using ProjectManagementApplication.Services.Implementations;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication_UnitTests
{
    public class ProjectsServiceTests
    {
        private List<ApplicationUser> exampleUsers = new List<ApplicationUser>
        { 
            new ApplicationUser() { Id = "id1" },
            new ApplicationUser() { Id = "id2" },
            new ApplicationUser() { Id = "id3" },
            new ApplicationUser() { Id = "id4" }
        };
        private Mock<IIdentityUserService> GetIdentityUsersServiceMock()
        {
            var identityUserServiceMock = new Mock<IIdentityUserService>();
            foreach (var user in exampleUsers)
            {
                identityUserServiceMock.Setup(s => s.FindByIdAsync(user.Id)).ReturnsAsync(user);
            }
            return identityUserServiceMock;
        }

        [Fact]
        public async Task CreateProjectAsync_NoDeveloperRoleAssigned_ReturnsValidationFailed()
        {
            var applicationDbContextStub = new Mock<IApplicationDbContext>();
            var identityUsersServiceMock = GetIdentityUsersServiceMock();
            // Only users in Product Owner and Scrum master roles are assigned, no developers
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[0])).ReturnsAsync(new List<string>() { "Scrum Master" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[1])).ReturnsAsync(new List<string>() { "Product Owner" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[2])).ReturnsAsync(new List<string>() { "Scrum Master" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[3])).ReturnsAsync(new List<string>() { "Product Owner" });
            ProjectsService service = new ProjectsService(applicationDbContextStub.Object, identityUsersServiceMock.Object);
            var request = new CreateProjectRequest()
            {
                Name = "",
                Description = "",
                SprintDuration = 0,
                UserIds = exampleUsers.Select(u => u.Id).ToList()
            };

            var result = await service.CreateProjectAsync(request);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
            identityUsersServiceMock.Verify(s => s.FindByIdAsync(It.IsAny<string>()), Times.AtLeast(4)); 
            identityUsersServiceMock.Verify(s => s.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Exactly(4));
        }
        [Fact]
        public async Task CreateProjectAsync_NoScrumMasterRoleAssigned_ReturnsValidationFailed()
        {
            var applicationDbContextStub = new Mock<IApplicationDbContext>();
            var identityUsersServiceMock = GetIdentityUsersServiceMock();
            // Only users in Product Owner and Scrum master roles are assigned, no developers
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[0])).ReturnsAsync(new List<string>() { "Developer" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[1])).ReturnsAsync(new List<string>() { "Product Owner" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[2])).ReturnsAsync(new List<string>() { "Developer" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[3])).ReturnsAsync(new List<string>() { "Product Owner" });
            ProjectsService service = new ProjectsService(applicationDbContextStub.Object, identityUsersServiceMock.Object);
            var request = new CreateProjectRequest()
            {
                Name = "",
                Description = "",
                SprintDuration = 0,
                UserIds = exampleUsers.Select(u => u.Id).ToList()
            };

            var result = await service.CreateProjectAsync(request);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
            identityUsersServiceMock.Verify(s => s.FindByIdAsync(It.IsAny<string>()), Times.AtLeast(4));
            identityUsersServiceMock.Verify(s => s.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Exactly(4));
        }
        [Fact]
        public async Task CreateProjectAsync_NoProductOwnerRoleAssigned_ReturnsValidationFailed()
        {
            var applicationDbContextStub = new Mock<IApplicationDbContext>();
            var identityUsersServiceMock = GetIdentityUsersServiceMock();
            // Only users in Product Owner and Scrum master roles are assigned, no developers
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[0])).ReturnsAsync(new List<string>() { "Developer" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[1])).ReturnsAsync(new List<string>() { "Scrum Master" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[2])).ReturnsAsync(new List<string>() { "Developer" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[3])).ReturnsAsync(new List<string>() { "Scrum Master" });
            ProjectsService service = new ProjectsService(applicationDbContextStub.Object, identityUsersServiceMock.Object);
            var request = new CreateProjectRequest()
            {
                Name = "",
                Description = "",
                SprintDuration = 0,
                UserIds = exampleUsers.Select(u => u.Id).ToList()
            };

            var result = await service.CreateProjectAsync(request);

            Assert.Equal(ResultStatus.ValidationFailed, result.Status);
            identityUsersServiceMock.Verify(s => s.FindByIdAsync(It.IsAny<string>()), Times.AtLeast(4));
            identityUsersServiceMock.Verify(s => s.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Exactly(4));
        }
        [Fact]
        public async Task CreateProjectAsync_AllRolesAssigned_ReturnsSuccess()
        {
            var applicationDbContextMock = new Mock<IApplicationDbContext>();
            applicationDbContextMock.Setup(x => x.Projects.Add(It.IsAny<Project>())).Returns(value: null!);
            var identityUsersServiceMock = GetIdentityUsersServiceMock();
            // Only users in Product Owner and Scrum master roles are assigned, no developers
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[0])).ReturnsAsync(new List<string>() { "Product Owner" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[1])).ReturnsAsync(new List<string>() { "Scrum Master" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[2])).ReturnsAsync(new List<string>() { "Developer" });
            identityUsersServiceMock.Setup(s => s.GetRolesAsync(exampleUsers[3])).ReturnsAsync(new List<string>() { "Developer" });
            ProjectsService service = new ProjectsService(applicationDbContextMock.Object, identityUsersServiceMock.Object);
            var request = new CreateProjectRequest()
            {
                Name = "",
                Description = "",
                SprintDuration = 0,
                UserIds = exampleUsers.Select(u => u.Id).ToList()
            };

            var result = await service.CreateProjectAsync(request);

            Assert.Equal(ResultStatus.Success, result.Status);
            identityUsersServiceMock.Verify(s => s.FindByIdAsync(It.IsAny<string>()), Times.AtLeast(4));
            identityUsersServiceMock.Verify(s => s.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Exactly(4));
        }
    }
}
