using AngleSharp.Html.Dom;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NuGet.Protocol;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication_IntegrationTests.Helpers;
using System.Net;
using System.Text.RegularExpressions;

namespace ProjectManagementApplication_IntegrationTests
{
    public class ProjectsTests
    {

        [Fact]
        public async Task Create_ValidRequest_AddsProjectAndReturnsSuccess()
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

            await UserHelper.SeedUsers(userManager, context);

            var users = userManager.Users.ToList();

            client.DefaultRequestHeaders.Add("Test-User", $"id:{users[0].Id};roles:Scrum Master;name:{users[0].UserName}");

            var resp = await client.GetAsync("/Projects/Create");

            var getHtml = await resp.Content.ReadAsStringAsync();

            var token = AntiForgeryTokenHelper.ExtractAntiForgeryToken(getHtml);

            var values = new List<KeyValuePair<string, string>>
            {
                new("Name", "My Project"),
                new("SprintDuration", "2"),
                new("__RequestVerificationToken", token)
            };
            foreach (var user in users)
            {
                values.Add(new KeyValuePair<string, string>("UserIds", user.Id));
            }
            var content = new FormUrlEncodedContent(values);
            var postResp = await client.PostAsync("/Projects/Create", content);

            using var scope2 = factory.Services.CreateScope();
            var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var exists = await context2.Projects.AnyAsync(p => p.Name == "My Project");

            Assert.True(exists);
            Assert.Equal(HttpStatusCode.OK, postResp.StatusCode);
        }

        [Theory]
        [InlineData("Developer")]
        [InlineData("Product Owner")]
        [InlineData("Scrum Master")]
        public async Task Create_NotAllRolesExist_DoesNotAddProjectAndReturnsView(string missingRole)
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

            await UserHelper.DeleteAllUsers(userManager, context);
            if (missingRole != "Developer") await UserHelper.SeedUser(userManager, context, "Developer", "dev@example.com");
            if (missingRole != "Scrum Master") await UserHelper.SeedUser(userManager, context, "Scrummaster", "sm@example.com", "Scrum Master");
            if (missingRole != "Product Owner") await UserHelper.SeedUser(userManager, context, "Productowner", "po@example.com", "Product Owner");

            var users = userManager.Users.ToList();

            client.DefaultRequestHeaders.Add("Test-User", $"id:{users[0].Id};roles:Scrum Master;name:{users[0].UserName}");

            var resp = await client.GetAsync("/Projects/Create");

            var getHtml = await resp.Content.ReadAsStringAsync();

            var token = AntiForgeryTokenHelper.ExtractAntiForgeryToken(getHtml);

            var values = new List<KeyValuePair<string, string>>
            {
                new("Name", "My Project"),
                new("SprintDuration", "2"),
                new("__RequestVerificationToken", token)
            };
            foreach (var user in users)
            {
                values.Add(new KeyValuePair<string, string>("UserIds", user.Id));
            }
            var content = new FormUrlEncodedContent(values);
            var postResp = await client.PostAsync("/Projects/Create", content);

            using var scope2 = factory.Services.CreateScope();
            var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var exists = await context2.Projects.AnyAsync(p => p.Name == "My Project");

            Assert.False(exists);
            Assert.Equal(HttpStatusCode.OK, postResp.StatusCode);
        }
    }
}