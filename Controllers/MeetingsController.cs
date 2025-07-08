using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Models.MeetingsViewModels;
using SQLitePCL;

namespace ProjectManagementApplication.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public MeetingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            List<Project> projects = await _context.Projects.Where(p => p.Users.Contains(user))
                .Include(p => p.Meetings
                    .Where(m => m.Time > DateTime.Now)
                    .OrderBy(m => m.Time))
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            List<ProjectSummaryViewModel> projectsVm = new List<ProjectSummaryViewModel>();

            foreach (var project in projects)
            {
                List<MeetingSummaryViewModel> meetingsVm = new List<MeetingSummaryViewModel>();
                foreach (Meeting meeting in project.Meetings)
                {
                    meetingsVm.Add(new MeetingSummaryViewModel()
                    {
                        Id = meeting.Id,
                        Name = meeting.Name,
                        Description = meeting.Description,
                        Time = meeting.Time.ToString("dd.MM.yyyy HH:mm"),
                        TypeOfMeeting = meeting.TypeOfMeeting.ToString()
                    });
                }

                projectsVm.Add(new ProjectSummaryViewModel()
                {
                    Id = project.Id,
                    Name = project.Name,
                    Meetings = meetingsVm
                });
            }

            MeetingsViewModel model = new MeetingsViewModel()
            {
                Projects = projectsVm
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int projectId)
        {
            Project? project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            var model = new CreateMeetingViewModel
            {
                ProjectId = projectId,
                Time = DateTime.Now,
                TypeOfMeetingOptions = Enum.GetValues(typeof(TypeOfMeeting))
                    .Cast<TypeOfMeeting>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = e.ToString()
                    })
            };

            return PartialView("_CreateMeeting", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMeetingViewModel model)
        {
            model.TypeOfMeetingOptions = Enum.GetValues(typeof(TypeOfMeeting))
                .Cast<TypeOfMeeting>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString(),
                    Selected = (e == model.TypeOfMeeting)
                });

            if (!ModelState.IsValid)
                return PartialView("_CreateMeeting", model);

            _context.Meetings.Add(new Meeting()
            {
                ProjectId = model.ProjectId,
                Name = model.Name,
                Description = model.Description,
                Time = model.Time,
                TypeOfMeeting = model.TypeOfMeeting
            });
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Meeting? meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return NotFound();

            var model = new CreateMeetingViewModel
            {
                Id = meeting.Id,
                ProjectId = meeting.ProjectId,
                Time = meeting.Time,
                Name = meeting.Name,
                Description = meeting.Description,
                TypeOfMeeting = meeting.TypeOfMeeting,
                TypeOfMeetingOptions = Enum.GetValues(typeof(TypeOfMeeting))
                    .Cast<TypeOfMeeting>()
                    .Select(e => new SelectListItem
                    {
                        Value = e.ToString(),
                        Text = e.ToString()
                    })
            };

            return PartialView("_EditMeeting", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateMeetingViewModel model)
        {
            model.TypeOfMeetingOptions = Enum.GetValues(typeof(TypeOfMeeting))
                .Cast<TypeOfMeeting>()
                .Select(e => new SelectListItem
                {
                    Value = e.ToString(),
                    Text = e.ToString(),
                    Selected = (e == model.TypeOfMeeting)
                });

            if (!ModelState.IsValid)
                return PartialView("_EditMeeting", model);

            _context.Meetings.Update(new Meeting()
            {
                Id = model.Id,
                ProjectId = model.ProjectId,
                Name = model.Name,
                Description = model.Description,
                Time = model.Time,
                TypeOfMeeting = model.TypeOfMeeting
            });
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Meeting? meeting = await _context.Meetings.FindAsync(id);
                if (meeting == null)
                {
                    return NotFound();
                }
                _context.Meetings.Remove(meeting);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
