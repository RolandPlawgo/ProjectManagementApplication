using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Common;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.MeetingsDtos;
using ProjectManagementApplication.Dto.Requests.MeetingRequests;
using ProjectManagementApplication.Models.MeetingsViewModels;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Controllers
{
    public class MeetingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMeetingsService _meetingsService;
        public MeetingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMeetingsService meetingsService)
        {
            _context = context;
            _userManager = userManager;
            _meetingsService = meetingsService;
        }
        public async Task<IActionResult> Index()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            
            MeetingsDto dto = await _meetingsService.GetMeetingsAsync(user);

            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "Scrum Master")]
        public IActionResult Create(int projectId)
        {
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

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Scrum Master")]
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

            var createMeetingRequest = new CreateMeetingRequest()
            {
                ProjectId = model.ProjectId,
                Name = model.Name,
                Description = model.Description,
                Time = model.Time,
                TypeOfMeeting = model.TypeOfMeeting
            };

            try
            {
                Result result = await _meetingsService.CreateMeetingAsync(createMeetingRequest);
                if (result.Status == ResultStatus.ValidationFailed)
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "");
                    return PartialView("_CreateMeeting", model);
                }
                else if (result.Status != ResultStatus.Success) return Json(new { success = false, error = result.ErrorMessage });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "An error occurred while creating the meeting." });
            }
            return Json(new { success = true });
        }

        [HttpGet]
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _meetingsService.GetForEditAsync(id);
            if (dto == null) return NotFound();

            var model = new EditMeetingViewModel
            {
                Id = dto.Id,
                ProjectId = dto.ProjectId,
                Time = dto.Time,
                Name = dto.Name,
                Description = dto.Description,
                TypeOfMeeting = dto.TypeOfMeeting,
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

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> Edit(EditMeetingViewModel model)
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

            
            var request = new EditMeetingRequest()
            {
                Id = model.Id,
                ProjectId = model.ProjectId,
                Name = model.Name,
                Description = model.Description,
                Time = model.Time,
                TypeOfMeeting = model.TypeOfMeeting
            };
            try
            {
                Result result = await _meetingsService.EditMeetingAsync(request);
                if (result.Status == ResultStatus.ValidationFailed)
                {
                    ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "");
                    return PartialView("_EditMeeting", model);
                }
                else if (result.Status != ResultStatus.Success) return Json(new { success = false, error = result.ErrorMessage });
            }
            catch (Exception)
            {
                return Json(new { success = false, error = "An error occured while savinf the meeting." });
            }
            return Json(new { success = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Scrum Master")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _meetingsService.DeleteMeetingAsync(id);
                return Json(new { success = success });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
