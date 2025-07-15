using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;

using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.V8;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using WebOptimizer;
using WebOptimizer.Sass;
using ProjectManagementApplication.Authentication;
using DartSassHost;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using ProjectManagementApplication.Data.Entities;

namespace ProjectManagementApplication
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            builder.Services.AddAuthorization(options =>
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

            builder.Services.AddControllers();
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToAreaFolder("Identity", "/Account");
            });


            builder.Services.AddScoped<IAuthorizationHandler, ProjectMemberHandler>();


            builder.Services.AddJsEngineSwitcher(options =>
            {
                options.DefaultEngineName = V8JsEngine.EngineName;
            })
            .AddV8();

            builder.Services.AddWebOptimizer(pipeline =>
            {
                pipeline.AddScssBundle("css/site.css", "scss/site.scss");
            });


            var app = builder.Build();



            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                context.Database.Migrate();

                const string demoName = "Sample Project";
                var sampleProject = await context.Projects
                    .Include(p => p.Users)
                    .Include(p => p.Epics)
                    .FirstOrDefaultAsync(p => p.Name == demoName);

                if (sampleProject == null)
                {
                    sampleProject = new Project
                    {
                        Name = demoName,
                        Description = "Sample project added by default",
                        SprintDuration = 2
                    };
                    context.Projects.Add(sampleProject);
                    await context.SaveChangesAsync();

                    var epicAuth = new Epic { ProjectId = sampleProject.Id, Title = "Authentication" };
                    var epicAuthz = new Epic { ProjectId = sampleProject.Id, Title = "Authorization" };
                    context.Epics.AddRange(epicAuth, epicAuthz);
                    await context.SaveChangesAsync();

                    context.UserStories.AddRange(
                        new UserStory
                        {
                            EpicId = epicAuth.Id,
                            Title = "As an anonymous user I want to register an account so that I can log in",
                            Description = "Provide a registration form and persist new users.",
                            Status = Status.Backlog
                        },
                        new UserStory
                        {
                            EpicId = epicAuth.Id,
                            Title = "As a registered user I want to reset my password so that I can recover access",
                            Description = "Implement password‐reset via email token.",
                            Status = Status.Backlog
                        },
                        new UserStory
                        {
                            EpicId = epicAuthz.Id,
                            Title = "As a manager I want to assign roles to users so that I control access levels",
                            Description = "Provide UI for role management.",
                            Status = Status.Backlog
                        },
                        new UserStory
                        {
                            EpicId = epicAuthz.Id,
                            Title = "As an administrator I want to view access logs so that I can audit security events",
                            Description = "Expose log viewer in admin panel.",
                            Status = Status.Backlog
                        }
                    );
                    await context.SaveChangesAsync();
                }

                if (!await roleManager.RoleExistsAsync("Scrum Master"))
                    await roleManager.CreateAsync(new IdentityRole("Scrum Master"));
                if (!await roleManager.RoleExistsAsync("Product Owner"))
                    await roleManager.CreateAsync(new IdentityRole("Product Owner"));

                async Task<ApplicationUser> CreateIfNotExists(string firstName, string email, string password, string? role = null)
                {
                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            FirstName = firstName,
                            LastName = "Demo"
                        };
                        await userManager.CreateAsync(user, password);
                        if (role != null)
                            await userManager.AddToRoleAsync(user, role);
                    }
                    return user;
                }

                var dev = await CreateIfNotExists("Developer", "developer@mail.com", "Qwerty1!");
                var scrumMaster = await CreateIfNotExists("Scrummaster", "scrummaster@mail.com", "Qwerty1!", "Scrum Master");
                var productOwner = await CreateIfNotExists("Productowner", "productowner@mail.com", "Qwerty1!", "Product Owner");

                foreach (var u in new[] { dev, scrumMaster, productOwner })
                {
                    if (!sampleProject.Users.Any(x => x.Id == u.Id))
                        sampleProject.Users.Add(u);
                }
                await context.SaveChangesAsync();
            }


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseWebOptimizer();


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Projects}/{action=Index}/{id?}");

            app.MapGet("/api/meetings/upcoming", async ([FromServices] ApplicationDbContext db, [FromQuery] int minutes) =>
            {
                var now = DateTime.Now;
                var cutoff = now.AddMinutes(minutes);

                var upcoming = await db.Meetings
                    .Where(m => m.Time >= now && m.Time <= cutoff)
                    .Include(m => m.Project)
                    .OrderBy(m => m.Time)
                    .Select(m => new {
                        m.Id,
                        m.Name,
                        Time = m.Time.ToString("dd.MM HH:mm"),
                        TypeOfMeeting = m.TypeOfMeeting.ToString(),
                        ProjectName = m.Project.Name,
                        ProjectId = m.ProjectId
                    })
                    .ToListAsync();

                return Results.Ok(upcoming);
            });


            app.MapRazorPages();


            app.Run();
        }
    }
}
