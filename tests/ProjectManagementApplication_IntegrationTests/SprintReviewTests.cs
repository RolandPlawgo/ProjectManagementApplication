using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication_IntegrationTests.Helpers;

namespace ProjectManagementApplication_IntegrationTests
{
    public class SprintReviewTests
    {
        [Fact]
        public async Task FinishSprint_ValidRequest_SetsSprintToNotActiveAndRedirectsTheUser()
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

            await UserHelper.SeedUser(userManager, context, "name", "user@mail.com", "Product Owner");
            var user = await userManager.FindByEmailAsync("user@mail.com");
            if (user == null) throw new Exception("Failed to create user");

            await context.Projects.AddAsync(new Project() { Name = "My Project", Description = "Description", SprintDuration = 2, Users = new List<ApplicationUser>() { user } });
            await context.SaveChangesAsync();
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Name == "My Project");
            await context.Sprints.AddAsync(new Sprint() { SprintGoal = "Sprint goal", ProjectId = project.Id, EndDate = DateTime.Now.AddDays(-1), Active = true });
            await context.SaveChangesAsync();
            Sprint sprint = context.Sprints.First();
            await context.Epics.AddAsync(new Epic() { ProjectId = project.Id, Title = "" });
            await context.SaveChangesAsync();
            Epic epic = context.Epics.First();
            await context.UserStories.AddAsync(new UserStory() { SprintId = sprint.Id, Description = "", Title = "", Status = Status.ProductIncrement, EpicId = epic.Id });
            await context.UserStories.AddAsync(new UserStory() { SprintId = sprint.Id, Description = "", Title = "", Status = Status.Backlog, EpicId = epic.Id });
            await context.SaveChangesAsync();

            client.DefaultRequestHeaders.Add("Test-User", $"id:{user.Id};roles:Product Owner;name:{user.FirstName}");

            var resp = await client.GetAsync($"/SprintReview/Index/{project.Id}");

            var getHtml = await resp.Content.ReadAsStringAsync();
            var token = AntiForgeryTokenHelper.ExtractAntiForgeryToken(getHtml);

            var values = new List<KeyValuePair<string, string>>
            {
                new("id", sprint.Id.ToString()),
                new("__RequestVerificationToken", token)
            };
            var content = new FormUrlEncodedContent(values);


            var postResp = await client.PostAsync("/SprintReview/FinishSprint", content);


            using var scope2 = factory.Services.CreateScope();
            var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Sprint? newSprint = await context2.Sprints.FirstOrDefaultAsync(p => p.Id == sprint.Id);
            bool active = newSprint?.Active ?? throw new Exception("Sprint does not exist");

            Assert.False(active);
            Assert.Equal(HttpStatusCode.Redirect, postResp.StatusCode);
        }
        [Fact]
        public async Task FinishSprint_NotAllTasksAcceptedOrRejected_ReturnsBadRequest()
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

            await UserHelper.SeedUser(userManager, context, "name", "user@mail.com", "Product Owner");
            var user = await userManager.FindByEmailAsync("user@mail.com");
            if (user == null) throw new Exception("Failed to create user");

            await context.Projects.AddAsync(new Project() { Name = "My Project", Description = "Description", SprintDuration = 2, Users = new List<ApplicationUser>() { user } });
            await context.SaveChangesAsync();
            var project = await context.Projects.FirstOrDefaultAsync(p => p.Name == "My Project");
            await context.Sprints.AddAsync(new Sprint() { SprintGoal = "Sprint goal", ProjectId = project.Id, EndDate = DateTime.Now.AddDays(-1), Active = true });
            await context.SaveChangesAsync();
            Sprint sprint = context.Sprints.First();
            await context.Epics.AddAsync(new Epic() { ProjectId = project.Id, Title = "" });
            await context.SaveChangesAsync();
            Epic epic = context.Epics.First();
            await context.UserStories.AddAsync(new UserStory() { SprintId = sprint.Id, Description = "", Title = "", Status = Status.ProductIncrement, EpicId = epic.Id });
            await context.UserStories.AddAsync(new UserStory() { SprintId = sprint.Id, Description = "", Title = "", Status = Status.Backlog, EpicId = epic.Id });
            await context.UserStories.AddAsync(new UserStory() { SprintId = sprint.Id, Description = "", Title = "", Status = Status.Sprint, EpicId = epic.Id }); // Status is set to sprint - the user story is neither accepted nor rejected
            await context.SaveChangesAsync();

            client.DefaultRequestHeaders.Add("Test-User", $"id:{user.Id};roles:Product Owner;name:{user.FirstName}");

            var resp = await client.GetAsync($"/SprintReview/Index/{project.Id}");

            var getHtml = await resp.Content.ReadAsStringAsync();
            var token = AntiForgeryTokenHelper.ExtractAntiForgeryToken(getHtml);

            var values = new List<KeyValuePair<string, string>>
            {
                new("id", sprint.Id.ToString()),
                new("__RequestVerificationToken", token)
            };
            var content = new FormUrlEncodedContent(values);


            var postResp = await client.PostAsync("/SprintReview/FinishSprint", content);


            using var scope2 = factory.Services.CreateScope();
            var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Sprint? newSprint = await context2.Sprints.FirstOrDefaultAsync(p => p.Id == sprint.Id);
            bool active = newSprint?.Active ?? throw new Exception("Sprint does not exist");

            Assert.True(active);
            Assert.Equal(HttpStatusCode.BadRequest, postResp.StatusCode);
        }
    }
}
