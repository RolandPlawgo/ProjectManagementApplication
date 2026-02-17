using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.MeetingsDtos;
using ProjectManagementApplication.Dto.Requests.MeetingRequests;
using ProjectManagementApplication.Models.MeetingsViewModels;
using ProjectManagementApplication.Services.Interfaces;

namespace ProjectManagementApplication.Services.Implementations
{
    public class MeetingsService : IMeetingsService
    {
        private readonly ApplicationDbContext _context;
        public MeetingsService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<MeetingsDto> GetMeetingsAsync(ApplicationUser user)
        {
            List<Project> projects = await _context.Projects.Where(p => p.Users.Contains(user))
                .Include(p => p.Meetings
                    .Where(m => m.Time > DateTime.Now)
                    .OrderBy(m => m.Time))
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            List<ProjectSummaryDto> projectsDto = new List<ProjectSummaryDto>();

            foreach (var project in projects)
            {
                List<MeetingSummaryDto> meetingsDto = new List<MeetingSummaryDto>();
                foreach (Meeting meeting in project.Meetings)
                {
                    meetingsDto.Add(new MeetingSummaryDto()
                    {
                        Id = meeting.Id,
                        Name = meeting.Name,
                        Description = meeting.Description,
                        Time = meeting.Time.ToString("dd.MM.yyyy HH:mm"),
                        TypeOfMeeting = meeting.TypeOfMeeting.ToString()
                    });
                }

                projectsDto.Add(new ProjectSummaryDto()
                {
                    Id = project.Id,
                    Name = project.Name,
                    Meetings = meetingsDto
                });
            }

            MeetingsDto dto = new MeetingsDto()
            {
                Projects = projectsDto
            };

            return dto;
        }

        public async Task CreateMeetingAsync(CreateMeetingRequest request)
        {
            _context.Meetings.Add(new Meeting()
            {
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                Time = request.Time,
                TypeOfMeeting = request.TypeOfMeeting
            });
            await _context.SaveChangesAsync();
        }

        public async Task<EditMeetingDto?> GetForEditAsync(int id)
        {
            Meeting? meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null) return null;

            var dto = new EditMeetingDto
            {
                Id = meeting.Id,
                ProjectId = meeting.ProjectId,
                Time = meeting.Time,
                Name = meeting.Name,
                Description = meeting.Description,
                TypeOfMeeting = meeting.TypeOfMeeting
            };

            return dto;
        }

        public async Task EditMeetingAsync(EditMeetingRequest request)
        {
            _context.Meetings.Update(new Meeting()
            {
                Id = request.Id,
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                Time = request.Time,
                TypeOfMeeting = request.TypeOfMeeting
            });
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteMeetingAsync(int id)
        {
            Meeting? meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return false;
            }
            _context.Meetings.Remove(meeting);
            _context.SaveChanges();
            return true;
        }
    }
}
