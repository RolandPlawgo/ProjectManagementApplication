using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Dto.Read.ProjectsDtos;
using ProjectManagementApplication.Dto.Requests.ProjectsRequests;
using ProjectManagementApplication.Models.ProjectsViewModels;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IProjectsService _projectsService;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuthorizationService authorizationService, IProjectsService projectsService)
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
            _projectsService = projectsService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null) return Forbid();

            var cards = await _projectsService.GetProjectsForUserAsync(userId);
            if (cards == null) return NotFound();

            return View(cards);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Forbid();

            ProjectDetailsDto? projectDetailsDto = await _projectsService.GetProjectDetailsAsync(id);
            if (projectDetailsDto == null) return NotFound();

            return PartialView("_Details", projectDetailsDto);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> Create()
        {
            var model = new CreateProjectViewModel { SprintDuration = 2 };
            await PopulateSelections(model);
            return PartialView("_Create", model);
        }

        // POST: Projects/Create
        [Authorize(Roles = "Scrum Master")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
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

            var createProjectRequest = new CreateProjectRequest
            {
                Name = model.Name,
                Description = model.Description,
                SprintDuration = model.SprintDuration,
                UserIds = model.UserIds
            };
            try
            {
                await _projectsService.CreateProjectAsync(createProjectRequest);
            }
            catch
            {
                ModelState.AddModelError(key: "", errorMessage: "An error occurred while creating the project. Please try again.");
                await PopulateSelections(model);
                return PartialView("_Create", model);
            }

            return Json(new { success = true });
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> Edit(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Forbid();

            var projectDto = await _projectsService.GetProjectDetailsAsync(id);
            if (projectDto == null) return NotFound();

            var model = new EditProjectViewModel
            {
                Id = projectDto.Id,
                Name = projectDto.Name,
                Description = projectDto.Description,
                SprintDuration = projectDto.SprintDuration,
                UserIds = projectDto.UsersWithRoles.Select(u => u.Id).ToList()
            };


            await PopulateSelections(model);

            return PartialView("_Edit", model);
        }

        // POST: Projects/Edit/5
        [Authorize(Roles = "Scrum Master")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditProjectViewModel model)
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

            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You’re not allowed to edit that project." });

            var editProjectRequest = new EditProjectRequest
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                SprintDuration = model.SprintDuration,
                UserIds = model.UserIds            };
            try
            {
                bool result = await _projectsService.UpdateProjectAsync(editProjectRequest);
                if (result == false)
                {
                    return Json(new { success = false, error = "Project not found." });
                }
            }
            catch
            {
                return Json(new { success = false, error = "An error occurred while updating the project. Please try again." });
            }

            return Json(new { success = true });
        }

        [Authorize(Roles = "Scrum Master")]
        [HttpPost, ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, resource: null, requirement: new ProjectMemberRequirement(id));
            if (!authResult.Succeeded) return Json(new { success = false, error = "You are not a member of this project." });

            var result = await _projectsService.DeleteProjectAsync(id);
            if (result == false) return Json(new { success = false, error = "Project not found." });

            return Json(new { success = true });
        }

        private async Task PopulateSelections(EditProjectViewModel vm)
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
        private async Task PopulateSelections(CreateProjectViewModel vm)
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
