using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.BacklogViewModels;
using SQLitePCL;

namespace ProjectManagementApplication.Controllers
{
    public class EpicsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EpicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(int projectId)
        {
            var vm = new EpicViewModel { ProjectId = projectId };
            return PartialView("_CreateEpic", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EpicViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateEpic", model);

            var epic = new Epic
            {
                Title = model.Title,
                ProjectId = model.ProjectId
            };
            _context.Epics.Add(epic);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var epic = await _context.Epics.FindAsync(id);
            if (epic == null) return NotFound();

            var vm = new EpicViewModel
            {
                Id = epic.Id,
                ProjectId = epic.ProjectId,
                Title = epic.Title
            };
            return PartialView("_EditEpic", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EpicViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditEpic", model);

            var epic = await _context.Epics.FindAsync(model.Id);
            if (epic == null) return NotFound();

            epic.Title = model.Title;
            _context.Epics.Update(epic);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var epic = await _context.Epics
                .Include(e => e.UserStories)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (epic == null) return Json(new { success = false });

            _context.UserStories.RemoveRange(epic.UserStories);
            _context.Epics.Remove(epic);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
