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
using ProjectManagementApplication.Models;

namespace ProjectManagementApplication.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            return View(await _context.Projects.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var project = await _context.Projects.FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var usersWithRoles = new List<dynamic>();
            foreach (var userId in project.UserId ?? Enumerable.Empty<string>())
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) continue;

                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Count() == 0)
                {
                    roles = new List<string>() { "User" };
                }
                usersWithRoles.Add(new
                {
                    UserName = user.UserName,
                    Roles = roles
                });
            }
            ViewBag.UsersWithRoles = usersWithRoles;

            return View(project);
        }
        
        // GET: Projects/Create
        [Authorize(Roles = "ScrumMaster")]
        public async Task<IActionResult> Create()
        {
            var project = new Project
            {
                SprintDuration = 2,
                UserId = new List<string>()
            };
            await PopulateSelections(project);

            return View(project);
        }

        // POST: Projects/Create
        [Authorize(Roles = "ScrumMaster")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,SprintDuration,UserId")] Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelections(project);

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "ScrumMaster")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            await PopulateSelections(project);

            return View(project);
        }

        // POST: Projects/Edit/5
        [Authorize(Roles = "ScrumMaster")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,SprintDuration,UserId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateSelections(project);

            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "ScrumMaster")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "ScrumMaster")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        private async Task PopulateSelections(Project project = null)
        {
            // 1) Sprint durations
            var durations = new[]
            {
            new SelectListItem("1 week", "1"),
            new SelectListItem("2 weeks", "2"),
            new SelectListItem("4 weeks", "4"),
            };

            var selectedDuration = project?.SprintDuration.ToString() ?? "2";
            ViewBag.SprintDurations =
                new SelectList(durations, "Value", "Text", selectedDuration);



            var users = _userManager.Users.ToList();

            var selectedUserIds = project?.UserId ?? new List<string>();

            var userItems = new List<SelectListItem>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                string label = user.UserName ?? "";
                if (roles.Count() > 0)
                {
                    label += $" ({string.Join(", ", roles)})";
                }

                userItems.Add(new SelectListItem
                {
                    Value = user.Id,
                    Text = label,
                    Selected = selectedUserIds.Contains(user.Id)
                });
            }

            ViewBag.AllUsers = userItems;
        }
    }
}
