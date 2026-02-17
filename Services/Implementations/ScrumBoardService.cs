using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using ProjectManagementApplication.Authentication;
using ProjectManagementApplication.Data;
using ProjectManagementApplication.Data.Entities;
using ProjectManagementApplication.Dto.Read.ScrumBoardDtos;
using ProjectManagementApplication.Dto.Requests.ScrumBoardRequests;
using ProjectManagementApplication.Services.Interfaces;
using System.Threading.Tasks;

namespace ProjectManagementApplication.Services.Implementations
{
    public class ScrumBoardService : IScrumBoardService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ScrumBoardService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ScrumBoardDto?> GetScrumBoardAsync(int sprintId)
        {
            Sprint? sprint = await _context.Sprints.Where(s => s.Id == sprintId)
                .Include(s => s.Project)
                .FirstOrDefaultAsync();
            if (sprint == null) return null;

                List<UserStory> userStories = await _context.UserStories
                .Where(u => u.SprintId == sprintId && u.Status == Status.Sprint)
                .Include(u => u.Subtasks).ThenInclude(s => s.Comments)
                .Include(u => u.Subtasks).ThenInclude(s => s.AssignedUser)
                .Include(u => u.Epic)
                .ToListAsync();
            List<UserStorySummaryDto> userStoriesVm = new List<UserStorySummaryDto>();
            List<SubtaskSummaryDto> toDoTasks = new List<SubtaskSummaryDto>();
            List<SubtaskSummaryDto> inProgressTasks = new List<SubtaskSummaryDto>();
            List<SubtaskSummaryDto> DoneTasks = new List<SubtaskSummaryDto>();
            foreach (UserStory userStory in userStories)
            {
                userStoriesVm.Add(new UserStorySummaryDto()
                {
                    Id = userStory.Id,
                    Title = userStory.Title,
                    EpicTitle = userStory.Epic.Title
                });

                foreach (Subtask subtask in userStory.Subtasks)
                {
                    if (subtask.AssignedUser == null && !subtask.Done)
                    {
                        toDoTasks.Add(new SubtaskSummaryDto()
                        {
                            Id = subtask.Id,
                            UserStoryId = userStory.Id,
                            Title = subtask.Title,
                            CommentsCount = subtask.Comments.Count()
                        });
                    }
                    if (subtask.AssignedUser != null && !subtask.Done)
                    {
                        inProgressTasks.Add(new SubtaskSummaryDto()
                        {
                            Id = subtask.Id,
                            UserStoryId = userStory.Id,
                            Title = subtask.Title,
                            CommentsCount = subtask.Comments.Count(),
                            AssignedUserInitials = Helpers.ApplicationUserHelper.UserInitials(subtask.AssignedUser)
                        });
                    }
                    if (subtask.AssignedUser != null && subtask.Done)
                    {
                        DoneTasks.Add(new SubtaskSummaryDto()
                        {
                            Id = subtask.Id,
                            UserStoryId = userStory.Id,
                            Title = subtask.Title,
                            CommentsCount = subtask.Comments.Count(),
                            AssignedUserInitials = Helpers.ApplicationUserHelper.UserInitials(subtask.AssignedUser)
                        });
                    }
                }
            }

            ScrumBoardDto dto = new ScrumBoardDto()
            {
                ProjectId = sprint.ProjectId,
                ProjectName = sprint.Project.Name,
                SprintId = sprint.Id,
                SprintGoal = sprint.SprintGoal,
                DaysToEndOfSprint = (sprint.EndDate - DateTime.Now)?.Days ?? 0,
                UserStories = userStoriesVm,
                ToDoTasks = toDoTasks,
                InProgressTasks = inProgressTasks,
                DoneTasks = DoneTasks
            };

            return dto;
        }

        public async Task<bool> MoveCardAsync(MoveCardRequest moveCardRequest)
        {
            Subtask? task = await _context.Subtasks
                .Where(s => s.Id == moveCardRequest.SubtaskId)
                .Include(t => t.UserStory)
                    .ThenInclude(u => u.Sprint)
                .FirstOrDefaultAsync();
            if (task == null) return false;
            

            if (moveCardRequest.TargetList == TargetList.todo)
            {
                task.AssignedUserId = null;
                task.AssignedUser = null;
                task.Done = false;
            }
            if (moveCardRequest.TargetList == TargetList.inprogress)
            {
                task.AssignedUserId = moveCardRequest.CurrentUserId;
                task.Done = false;
            }
            if (moveCardRequest.TargetList == TargetList.done)
            {
                task.AssignedUserId = moveCardRequest.CurrentUserId;
                task.Done = true;
            }
            _context.Subtasks.Update(task);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> FinishSprintEarlyAsync(int id)
        {
            Sprint? sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return false;

            sprint.EndDate = DateTime.Now;

            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync();

            return true;
        }
    }

}
