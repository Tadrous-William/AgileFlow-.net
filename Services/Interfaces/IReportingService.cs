using AgileTaskManager.Models.ViewModels;
using System.Threading.Tasks;

namespace AgileTaskManager.Services.Interfaces;

public interface IReportingService
{
    Task<ReportGenerationViewModel> GenerateReportAsync(ReportRequestViewModel request);
    Task<List<ReportTemplateViewModel>> GetReportTemplatesAsync();
    Task<ReportTemplateViewModel> GetReportTemplateAsync(int templateId);
    Task<ReportTemplateViewModel> CreateReportTemplateAsync(CreateReportTemplateViewModel request);
    Task<bool> UpdateReportTemplateAsync(int templateId, UpdateReportTemplateViewModel request);
    Task<bool> DeleteReportTemplateAsync(int templateId);
    Task<List<ScheduledReportViewModel>> GetScheduledReportsAsync();
    Task<ScheduledReportViewModel> GetScheduledReportAsync(int reportId);
    Task<ScheduledReportViewModel> CreateScheduledReportAsync(CreateScheduledReportViewModel request);
    Task<bool> UpdateScheduledReportAsync(int reportId, UpdateScheduledReportViewModel request);
    Task<bool> DeleteScheduledReportAsync(int reportId);
    Task<bool> ExecuteScheduledReportAsync(int reportId);
    Task<List<ReportHistoryViewModel>> GetReportHistoryAsync(string? userId = null, int count = 50);
    Task<ReportHistoryViewModel> GetReportHistoryAsync(int historyId);
    Task<byte[]> ExportReportAsync(ReportGenerationViewModel report, string format);
    Task<ReportPreviewViewModel> PreviewReportAsync(ReportRequestViewModel request);
    Task<ReportStatisticsViewModel> GetReportStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<CustomFieldViewModel>> GetCustomFieldsAsync(string reportType);
    Task<bool> SaveCustomFieldsAsync(string reportType, List<CustomFieldViewModel> fields);
    Task<ReportValidationViewModel> ValidateReportRequestAsync(ReportRequestViewModel request);
    Task<ReportPermissionViewModel> GetReportPermissionsAsync(string userId);
    Task<bool> GrantReportPermissionAsync(string userId, string reportType, string permission);
    Task<bool> RevokeReportPermissionAsync(string userId, string reportType, string permission);
}
