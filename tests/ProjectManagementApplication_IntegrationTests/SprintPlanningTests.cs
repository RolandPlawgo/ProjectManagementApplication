using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication_IntegrationTests.Helpers;
using System.Net;

namespace ProjectManagementApplication_IntegrationTests
{
    public class SprintPlanningTests
    {
        [Fact]
        public async Task StartSprint_ValidRequest_SetsSprintToActiveAndRedirectsTheUser()
        {
            await using var factory = new ProjectManagementApplicationFactory();
            await factory.InitializeAsync();

            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await UserHelper.SeedUser(userManager, context, "name", "user@mail.com", "Scrum Master");
            var user = await userManager.FindByEmailAsync("user@mail.com");
            if (user == null) throw new Exception("Failed to create user");

            await context.Projects.AddAsync(new Project() { Name = "My Project", Description = "Description", SprintDuration = 2, Users = new List<ApplicationUser>() { user } });
            await context.SaveChangesAsync();
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Name == "My Project");
            await context.Sprints.AddAsync(new Sprint() { SprintGoal = "Sprint goal", ProjectId = project.Id });
            await context.SaveChangesAsync();

            client.DefaultRequestHeaders.Add("Test-User", $"id:{user.Id};roles:Scrum Master;name:{user.FirstName}");

            var resp = await client.GetAsync($"/SprintPlanning/Index/{project.Id}");

            var getHtml = await resp.Content.ReadAsStringAsync();
            var token = AntiForgeryTokenHelper.ExtractAntiForgeryToken(getHtml);

            Sprint sprint = context.Sprints.First();
            var values = new List<KeyValuePair<string, string>>
            {
                new("id", sprint.Id.ToString()),
                new("__RequestVerificationToken", token)
            };
            var content = new FormUrlEncodedContent(values);


            var postResp = await client.PostAsync("/SprintPlanning/StartSprint", content);


            using var scope2 = factory.Services.CreateScope();
            var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Sprint? newSprint = await context2.Sprints.FirstOrDefaultAsync(p => p.Id == sprint.Id);
            bool active = newSprint?.Active ?? throw new Exception("Sprint does not exist");

            Assert.True(active);
            Assert.Equal(HttpStatusCode.Redirect, postResp.StatusCode);
        }
        [Fact]
        public async Task StartSprint_ValidRequest_SetsStartAndEndDate()
        {
            await using var factory = new ProjectManagementApplicationFactory();
            await factory.InitializeAsync();

            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await UserHelper.SeedUser(userManager, context, "name", "user@mail.com", "Scrum Master");
            var user = await userManager.FindByEmailAsync("user@mail.com");
            if (user == null) throw new Exception("Failed to create user");

            await context.Projects.AddAsync(new Project() { Name = "My Project", Description = "Description", SprintDuration = 2, Users = new List<ApplicationUser>() { user } });
            await context.SaveChangesAsync();
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Name == "My Project");
            await context.Sprints.AddAsync(new Sprint() { SprintGoal = "Sprint goal", ProjectId = project.Id });
            await context.SaveChangesAsync();

            client.DefaultRequestHeaders.Add("Test-User", $"id:{user.Id};roles:Scrum Master;name:{user.FirstName}");

            var resp = await client.GetAsync($"/SprintPlanning/Index/{project.Id}");

            var getHtml = await resp.Content.ReadAsStringAsync();
            var token = AntiForgeryTokenHelper.ExtractAntiForgeryToken(getHtml);

            Sprint sprint = context.Sprints.First();
            var values = new List<KeyValuePair<string, string>>
            {
                new("id", sprint.Id.ToString()),
                new("__RequestVerificationToken", token)
            };
            var content = new FormUrlEncodedContent(values);


            var postResp = await client.PostAsync("/SprintPlanning/StartSprint", content);


            using var scope2 = factory.Services.CreateScope();
            var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Sprint? newSprint = await context2.Sprints.FirstOrDefaultAsync(p => p.Id == sprint.Id);
            DateTime? startDate = newSprint?.StartDate ?? throw new Exception("Sprint does not exist");
            DateTime? endDate = newSprint?.EndDate ?? throw new Exception("Sprint does not exist");

            Assert.NotNull(startDate);
            Assert.NotNull(endDate);
            Assert.True(endDate > startDate, "The end date must be greater than the start date");
            Assert.True(startDate <= DateTime.Now, "The start date must be lower or equal to the current time");
            Assert.True(endDate >= DateTime.Now, "The end date must be greater or equal to the current time");
            Assert.True(endDate <= DateTime.Now.AddDays(project.SprintDuration * 7), "The end date is not correct");
        }
    }
}
