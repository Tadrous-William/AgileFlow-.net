using AgileTaskManager.Models.ViewModels;

namespace AgileTaskManager.Services.Interfaces;

public interface ISprintService
{
    Task<SprintViewModel> CreateAsync(CreateSprintViewModel model, string createdBy);
    Task<SprintViewModel> UpdateAsync(UpdateSprintViewModel model, string updatedBy);
    Task<bool> DeleteAsync(int sprintId, string userId);
    Task<SprintViewModel?> GetByIdAsync(int sprintId);
    Task<List<SprintViewModel>> GetByProjectAsync(int projectId);
    Task<SprintViewModel?> GetCurrentSprintAsync(int projectId);
    Task<List<SprintViewModel>> GetActiveSprintsAsync(int projectId);
    Task<bool> StartSprintAsync(int sprintId, string userId);
    Task<bool> CompleteSprintAsync(int sprintId, string userId);
    Task<SprintAnalyticsViewModel> GetSprintAnalyticsAsync(int sprintId);
    Task<ProjectAnalyticsViewModel> GetProjectAnalyticsAsync(int projectId);
    Task<List<SprintBurndownViewModel>> GetBurndownDataAsync(int sprintId);
}
