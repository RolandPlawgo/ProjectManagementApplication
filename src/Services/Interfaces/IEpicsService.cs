using ProjectManagementApplication.Dto.Read.EpicsDtos;
using ProjectManagementApplication.Dto.Requests.EpicsViewModels;

namespace ProjectManagementApplication.Services.Interfaces
{
    public interface IEpicsService
    {
        public Task CreateEpicAsync(CreateEpicRequest createEpicRequest);
        public Task<EditEpicDto?> GetEpicForEditAsync(int id);
        public Task<bool> EditEpicAsync(EditEpicRequest editEpicRequest);
        public Task<bool> DeleteEpicAsync(int id);

        public Task<int?> GetProjectIdForEpicAsync(int epicId);

    }
}
