using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUglify.JavaScript.Syntax;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Helpers;
using System.Drawing.Printing;

namespace ProjectManagementApplication.Controllers
{
    public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		public UsersController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

        private const int pageSize = 10;

		// GET: UsersController
		public ActionResult Index( int page = 1)
		{
			int totalUsers = _userManager.Users.Count();
			ViewBag.CurrentPage = page;
			int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
			ViewBag.TotalPages = totalPages;

			var users = _userManager.Users
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			return View(users);
        }

        // GET: UsersController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UsersController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UsersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string role)
        {
            user.UserName = user.Email;
            string tempPassword = PasswordHelper.GeneratePassword(_userManager.Options.Password);

            var result = await _userManager.CreateAsync(user, tempPassword);

            if (result.Succeeded)
            {
                if (role == "ScrumMaster" || role == "ProductOwner")
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
                    return View();
                }
            }

            return View(user);
        }

        // GET: UsersController/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			return View(user);
        }

        // POST: UsersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser model, string role)
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
            if (role == "ScrumMaster" || role == "ProductOwner")
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
                return View();
            }
        }

        // POST: UsersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, IFormCollection collection)
        {
            ApplicationUser? user = _userManager.Users.Where(u => u.Id == id).FirstOrDefault();
            if (user is null)
            {
                return NotFound();
            }
            await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
		}
	}
}
