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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

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

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();



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



            using (var serviceScope = app.Services.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                await roleManager.CreateAsync(new IdentityRole("Scrum Master"));
                await roleManager.CreateAsync(new IdentityRole("Product Owner"));

                var developer = new ApplicationUser()
                {
                    FirstName = "Adam",
                    LastName = "Nowak",
                    Email = "adamnowak@mail.com",
                    UserName = "adamnowak@mail.com"
                };
                await userManager.CreateAsync(developer, "Qwerty1!");

                var scrumMaster = new ApplicationUser()
                {
                    FirstName = "Jan",
                    LastName = "Kowalski",
                    Email = "jankowalski@mail.com",
                    UserName = "jankowalski@mail.com"
                };
                await userManager.CreateAsync(scrumMaster, "Qwerty1!");
                await userManager.AddToRoleAsync(scrumMaster, "Scrum Master");

                var productOwner = new ApplicationUser()
                {
                    FirstName = "Anna",
                    LastName = "Wiśniewska",
                    Email = "annawisniwska@mail.com",
                    UserName = "annawisniwska@mail.com"
                };
                await userManager.CreateAsync(productOwner, "Qwerty1!");
                await userManager.AddToRoleAsync(productOwner, "Product Owner");
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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Projects}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
