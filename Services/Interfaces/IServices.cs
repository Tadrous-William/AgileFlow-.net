using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.Enums;
using AgileTaskManager.Models.ViewModels;

namespace AgileTaskManager.Services.Interfaces;

public interface ITaskService
{
    Task<List<TaskListViewModel>> GetAllAsync(string? userId = null, string? role = null);
    Task<TaskDetailsViewModel?> GetDetailsAsync(int id);
    Task<TaskItem> CreateAsync(TaskCreateViewModel vm, string creatorId);
    Task UpdateAsync(TaskEditViewModel vm, string actorId);
    Task<bool> DeleteAsync(int id);
    Task AssignAsync(int taskId, string userId, string actorId);
    Task UpdateStatusAsync(int taskId, Models.Enums.TaskStatus newStatus, string actorId);
    Task<bool> IsBlockedAsync(int taskId);
    Task<List<TaskListViewModel>> GetByUserAsync(string userId);
}

public interface ICommentService
{
    Task<Comment> AddAsync(AddCommentViewModel vm, string userId);
    Task DeleteAsync(int id, string requestingUserId, bool isAdmin);
    Task<List<CommentViewModel>> GetByTaskAsync(int taskId);
}

public interface IFeedbackService
{
    Task<Feedback> AddAsync(AddFeedbackViewModel vm, string clientId);
    Task<FeedbackViewModel?> GetByTaskAsync(int taskId);
}

public interface INotificationService
{
    Task CreateAsync(string userId, string message, string? link = null);
    Task<List<NotificationViewModel>> GetUserNotificationsAsync(string userId);
    Task MarkAsReadAsync(int id, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}

public interface IAuditService
{
    Task LogAsync(int taskId, string actorId, string action, string? oldValue = null, string? newValue = null);
    Task<List<ActivityLogViewModel>> GetByTaskAsync(int taskId);
    Task<List<ActivityLogViewModel>> GetRecentAsync(int count = 20);
}

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
    Task SendTaskAssignedAsync(string toEmail, string toName, string taskTitle);
    Task SendTaskCompletedAsync(string toEmail, string toName, string taskTitle);
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
}

public interface IExportService
{
    byte[] ExportTasksToExcel(List<TaskListViewModel> tasks);
    Task<byte[]> ExportTasksToPdfAsync(List<TaskListViewModel> tasks);
}

public interface IDashboardService
{
    Task<DashboardViewModel> GetAdminDashboardAsync();
    Task<DashboardViewModel> GetMemberDashboardAsync(string userId);
    Task<DashboardViewModel> GetClientDashboardAsync(string clientId);
    
    // New role-specific dashboard methods
    Task<AdminDashboardViewModel> GetAdminDashboardEnhancedAsync();
    Task<TeamLeadDashboardViewModel> GetTeamLeadDashboardAsync(string userId);
    Task<DeveloperDashboardViewModel> GetDeveloperDashboardAsync(string userId);
    Task<ClientDashboardViewModel> GetClientDashboardEnhancedAsync(string userId);
}

public interface IUserService
{
    Task<List<UserListViewModel>> GetAllAsync();
    Task<UserListViewModel?> GetByIdAsync(string id);
    Task AssignRoleAsync(string userId, string newRole);
    Task ToggleActiveAsync(string userId);
}

public interface IAIAssistantService
{
    Task<string> GenerateTaskDescriptionAsync(string title, string? projectContext = null);
    Task<string> SuggestTaskTitleAsync(string description);
    Task<List<string>> SuggestTaskTagsAsync(string title, string description);
}



public interface ITaskDependencyService
{
    Task<DependencyGraphViewModel> GetDependencyGraphAsync(int taskId);
    Task<DependencyValidationResult> ValidateDependencyAsync(int taskId, int dependsOnTaskId);
    Task<bool> CreateDependencyAsync(int taskId, int dependsOnTaskId);
    Task<bool> RemoveDependencyAsync(int taskId, int dependsOnTaskId);
    Task<List<TaskDependencyViewModel>> GetBlockingTasksAsync(int taskId);
    Task<List<TaskDependencyViewModel>> GetDependentTasksAsync(int taskId);
    Task<bool> UpdateDependenciesAsync(int taskId, List<int> dependsOnTaskIds);
    Task<bool> CanStartTaskAsync(int taskId);
}

public interface ITimeTrackingService
{
    Task<TimeEntryViewModel> StartTimerAsync(int taskId, string userId, string? description = null);
    Task<TimeEntryViewModel> StopTimerAsync(int taskId, string userId);
    Task<TimeEntryViewModel> GetActiveTimerAsync(string userId);
    Task<List<TimeEntryViewModel>> GetUserTimeEntriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<TimeEntryViewModel> CreateManualTimeEntryAsync(CreateTimeEntryViewModel model, string userId);
    Task<TimeEntryViewModel> UpdateTimeEntryAsync(UpdateTimeEntryViewModel model, string userId);
    Task<bool> DeleteTimeEntryAsync(int entryId, string userId);
    Task<TimeTrackingDashboardViewModel> GetDashboardAsync(string userId);
    Task<TimeSheetViewModel> GetTimeSheetAsync(string userId, DateTime startDate, DateTime endDate);
    Task<TimeAnalyticsViewModel> GetTimeAnalyticsAsync(DateTime startDate, DateTime endDate, int? projectId = null, string? userId = null);
    Task<TimeReportViewModel> GenerateTimeReportAsync(CreateTimeReportViewModel model, string generatedBy);
}

public interface IMeetingService
{
    Task<MeetingViewModel> CreateMeetingAsync(CreateMeetingViewModel model, string createdBy);
    Task<MeetingViewModel> UpdateMeetingAsync(UpdateMeetingViewModel model, string updatedBy);
    Task<bool> DeleteMeetingAsync(int meetingId, string userId);
    Task<MeetingViewModel> GetMeetingAsync(int meetingId);
    Task<List<MeetingViewModel>> GetMeetingsAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
    Task<MeetingViewModel> StartMeetingAsync(int meetingId, string startedBy);
    Task<MeetingViewModel> EndMeetingAsync(int meetingId, string endedBy);
    Task<bool> JoinMeetingAsync(int meetingId, string userId);
    Task<bool> LeaveMeetingAsync(int meetingId, string userId);
    Task<StandupNoteViewModel> CreateStandupNoteAsync(CreateStandupNoteViewModel model, string userId);
    Task<List<StandupNoteViewModel>> GetStandupNotesAsync(int projectId, DateTime? date = null);
    Task<RetrospectiveNoteViewModel> CreateRetrospectiveNoteAsync(CreateRetrospectiveNoteViewModel model, string userId);
    Task<List<RetrospectiveNoteViewModel>> GetRetrospectiveNotesAsync(int meetingId);
    Task<MeetingActionItemViewModel> CreateActionItemAsync(int meetingId, CreateActionItemViewModel model, string createdBy);
    Task<MeetingActionItemViewModel> UpdateActionItemAsync(UpdateActionItemViewModel model, string updatedBy);
    Task<bool> DeleteActionItemAsync(int actionItemId, string userId);
    Task<MeetingDashboardViewModel> GetDashboardAsync(string userId);
    Task<MeetingAnalyticsViewModel> GetAnalyticsAsync(int projectId, DateTime startDate, DateTime endDate);
}
