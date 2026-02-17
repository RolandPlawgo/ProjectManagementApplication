using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Dto.Read.UsersDtos;
using ProjectManagementApplication.Dto.Requests.UsersRequests;
using ProjectManagementApplication.Helpers;
using ProjectManagementApplication.Models.UsersViewModels;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Controllers
{
    [Authorize(Roles = "Scrum Master")]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUsersService _usersService;
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUsersService usersService)
		{
			_userManager = userManager;
            _roleManager = roleManager;
            _usersService = usersService;
		}

        private const int pageSize = 10;

		public async Task<ActionResult> Index( int page = 1)
		{
			int totalUsers = _userManager.Users.Count();
			ViewBag.CurrentPage = page;
			int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
			ViewBag.TotalPages = totalPages;

            return View(await _usersService.GetUsersAsync(page, pageSize));
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create()
        {
            List<string> rolesOptions = new List<string>()
            {
                "Developer"
            };
            var roles = _roleManager.Roles
                            .Select(r => r.Name ?? "")
                            .ToList();
            if (roles == null)
            {
                return NotFound();   
            }
            rolesOptions.AddRange(roles);

            CreateUserViewModel userVm = new CreateUserViewModel()
            {
                RolesOptions = rolesOptions
            };
            return View(userVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel userVm, string role)
        {
            var request = new CreateUserRequest()
            {
                Email = userVm.Email,
                FirstName = userVm.FirstName,
                LastName = userVm.LastName,
                Role = role
            };

            string? tempPassword = await _usersService.CreateUserWithTempPasswordAsync(request);
            if(tempPassword != null)
            {
                TempData["UserEmail"] = userVm.Email;
                TempData["TempPassword"] = tempPassword;

                return View("CreateConfirmation");
            }
            else
            {
                return View(userVm);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            List<string> rolesOptions = new List<string>()
            {
                "Developer"
            };
            var roles = _roleManager.Roles
                            .Select(r => r.Name ?? "")
                            .ToList();
            if (roles == null)
            {
                return NotFound();
            }
            rolesOptions.AddRange(roles);

            EditUserDto? dto = await _usersService.GetForEditAsync(id);
            if (dto == null)
            {
                return NotFound();
            }

            EditUserViewModel model = new EditUserViewModel()
            {
                Id = dto.Id,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                RolesOptions = rolesOptions
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CreateUserViewModel model, string role)
        {
            var request = new EditUserRequest()
            {
                Id = id,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = role
            };
            if (!await _usersService.EditUserAsync(request))
                return NotFound();
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                ApplicationUser? user = _userManager.Users.Where(u => u.Id == id).FirstOrDefault();
                if (user is null)
                {
                    return NotFound();
                }
                await _userManager.DeleteAsync(user);

                return Json(new { success = true });
            }
			catch
			{
				return Json(new { success = false });
			}
		}
	}
}
