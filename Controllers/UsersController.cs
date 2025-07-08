using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Helpers;
using ProjectManagementApplication.Models.UsersViewModels;

namespace ProjectManagementApplication.Controllers
{
	[Authorize(Roles = "Scrum Master")]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
            _roleManager = roleManager;
		}

        private const int pageSize = 10;

		public async Task<ActionResult> Index( int page = 1)
		{
			int totalUsers = _userManager.Users.Count();
			ViewBag.CurrentPage = page;
			int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
			ViewBag.TotalPages = totalPages;

			var users = await _userManager.Users
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

            List<UsersViewModel> usersVm = new List<UsersViewModel>();
            foreach (ApplicationUser user in users)
            {
                string role = "";
                List<string> roles = (await _userManager.GetRolesAsync(user)).ToList();
                if (!roles.Contains("Scrum Master") && !roles.Contains("Product Owner"))
                {
                    role = "Developer";
                }
                else
				{
					role = roles.FirstOrDefault() ?? "";
				}

                usersVm.Add(new UsersViewModel()
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? ""
				});
            }
            return View(usersVm);
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
            var user = new ApplicationUser
            {
                UserName = userVm.Email,
                Email = userVm.Email,
                FirstName = userVm.FirstName,
                LastName = userVm.LastName
            };

            string tempPassword = PasswordHelper.GeneratePassword(_userManager.Options.Password);

            var result = await _userManager.CreateAsync(user, tempPassword);

            if (result.Succeeded)
            {
                if (role == "Scrum Master" || role == "Product Owner")
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                user.MustChangePassword = true;
                await _userManager.UpdateAsync(user);

                TempData["UserEmail"] = user.Email;
                TempData["TempPassword"] = tempPassword;

                try
                {
                    return View("CreateConfirmation");
                }
                catch
                {
                    return View(userVm);
                }
            }

            return View(userVm);
        }

        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

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
                Id = id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "",
                RolesOptions = rolesOptions
            };
            return View(userVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CreateUserViewModel model, string role)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (role == "Scrum Master" || role == "Product Owner")
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            await _userManager.UpdateAsync(user);
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
