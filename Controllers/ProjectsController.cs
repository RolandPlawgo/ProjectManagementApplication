using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.ProjectsViewModels;

namespace ProjectManagementApplication.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService)
        {
            _context = context;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            List<Project> projects = new List<Project>();
				var currentUser = await _userManager.GetUserAsync(User);
				var userId = currentUser?.Id;

				projects = await _context.Projects
					.Include(p => p.Users)
					.Where(p => p.Users.Any(u => u.Id == userId))
					.ToListAsync();

            var cards = new List<ProjectCardViewModel>();
            foreach (var p in projects)
            {
                var model = new ProjectCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                };
                foreach (var user in p.Users)
                {
                    model.UserInitials.Add(Helpers.ApplicationUserHelper.UserInitials(user));
                }
                cards.Add(model);
            }

            return View(cards);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Forbid();

            var model = new ProjectDetailsViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                SprintDuration = project.SprintDuration
            };

            foreach (var user in project.Users)
            {
				var role = (await _userManager.GetRolesAsync(user)).DefaultIfEmpty("Developer").FirstOrDefault();
				model.UsersWithRoles.Add(new UserWithRolesViewModel
                {
                    UserName = user.UserName ?? "",
                    Role = role ?? ""
                });
            }
            if (model == null) return NotFound();

            return PartialView("_Details", model);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> Create()
        {
            var model = new ProjectEditViewModel { SprintDuration = 2 };
            await PopulateSelections(model);
            return PartialView("_Create", model);
        }

        // POST: Projects/Create
        [Authorize(Roles = "Scrum Master")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelections(model);
                return PartialView("_Create", model);
            }
            if (!await AllRolesExistInProject(model.UserIds))
            {
                ModelState.AddModelError(
                    key: "",
                    errorMessage: "You must assign at least one Scrum Master, one Product Owner and one Developer."
                );
                await PopulateSelections(model);
                return PartialView("_Create", model);
            }

            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                SprintDuration = model.SprintDuration
            };

            foreach (var userId in model.UserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    project.Users.Add(user);
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Forbid();

            var model = new ProjectEditViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                SprintDuration = project.SprintDuration,
                UserIds = project.Users.Select(u => u.Id).ToList()
            };
            await PopulateSelections(model);

            return PartialView("_Edit", model);
        }

        // POST: Projects/Edit/5
        [Authorize(Roles = "Scrum Master")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectEditViewModel model)
        {
            if (id != model.Id || !ModelState.IsValid)
            {
                await PopulateSelections(model);
                return PartialView("_Edit", model);
            }
            if (!await AllRolesExistInProject(model.UserIds))
            {
                ModelState.AddModelError(
                    key: "",
                    errorMessage: "You must assign at least one Scrum Master, one Product Owner and one Developer."
                );
                await PopulateSelections(model);
                return PartialView("_Edit", model);
            }

            var project = await _context.Projects
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == model.Id);
            if (project == null) return Json(new { success = false, error = "Project not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You’re not allowed to edit that project." });

            project.Name = model.Name;
            project.Description = model.Description;
            project.SprintDuration = model.SprintDuration;

            project.Users.Clear();
            foreach (var userId in model.UserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    project.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [Authorize(Roles = "Scrum Master")]
        [HttpPost, ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return Json(new { success = false, error = "Project not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private async Task PopulateSelections(ProjectEditViewModel vm)
        {
            vm.SprintDurations = new List<SelectListItem>
            {
                new SelectListItem("1 week", "1"),
                new SelectListItem("2 weeks", "2"),
                new SelectListItem("4 weeks", "4")
            };

            var users = await _userManager.Users.ToListAsync();
            vm.AllUsers = users.Select(u =>
            {
                var roles = _userManager.GetRolesAsync(u).Result;
                var label = string.IsNullOrEmpty(u.UserName) ? "" : u.UserName;
                if (roles.Any())
                    label += $" ({string.Join(", ", roles)})";
                return new SelectListItem
                {
                    Value = u.Id,
                    Text = label,
                    Selected = vm.UserIds.Contains(u.Id)
                };
            }).ToList();
        }

        private async Task<bool> AllRolesExistInProject(List<string> userIds)
        {
            bool hasDev = false, hasPo = false, hasSm = false;

            foreach (var uid in userIds)
            {
                var user = await _userManager.FindByIdAsync(uid);
                if (user == null) throw new InvalidOperationException("User not found");
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Product Owner")) hasPo = true;
                else if (roles.Contains("Scrum Master")) hasSm = true;
                else hasDev = true;
            }

            return hasDev && hasPo && hasSm;
        }
    }
}
