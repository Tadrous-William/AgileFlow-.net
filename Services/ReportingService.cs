using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AgileTaskManager.Services;

public class ReportingService : IReportingService
{
    private readonly ApplicationDbContext _db;

    public ReportingService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ReportGenerationViewModel> GenerateReportAsync(ReportRequestViewModel request)
    {
        // Generate report based on request parameters
        var reportData = new Dictionary<string, object>
        {
            ["GeneratedAt"] = DateTime.UtcNow,
            ["Parameters"] = request
        };

        var report = new ReportHistory
        {
            ReportName = request.ReportType,
            ReportType = request.ReportType,
            GeneratedBy = "System",
            GeneratedAt = DateTime.UtcNow,
            Status = "Completed",
            Format = "PDF"
        };

        _db.ReportHistories.Add(report);
        await _db.SaveChangesAsync();

        return new ReportGenerationViewModel
        {
            ReportId = report.Id.ToString(),
            ReportName = report.ReportName,
            ReportType = report.ReportType,
            GeneratedAt = report.GeneratedAt,
            Status = report.Status
        };
    }

    public async Task<List<ReportTemplateViewModel>> GetReportTemplatesAsync()
    {
        return await _db.ReportTemplates
            .Where(rt => rt.IsActive)
            .Select(rt => new ReportTemplateViewModel
            {
                Id = rt.Id,
                Name = rt.Name,
                Description = rt.Description,
                ReportType = rt.ReportType,
                Parameters = string.IsNullOrEmpty(rt.Parameters)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(rt.Parameters, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>(),
                CreatedAt = rt.CreatedAt,
                UpdatedAt = rt.UpdatedAt,
                CreatedBy = rt.CreatedBy,
                IsActive = rt.IsActive,
                DefaultFormat = rt.DefaultFormat,
                ScheduleFrequency = rt.ScheduleFrequency,
                Recipients = string.IsNullOrEmpty(rt.Recipients)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(rt.Recipients, (JsonSerializerOptions?)null) ?? new List<string>()
            })
            .ToListAsync();
    }

    public async Task<ReportTemplateViewModel> GetReportTemplateAsync(int templateId)
    {
        var template = await _db.ReportTemplates.FindAsync(templateId);
        if (template == null)
            throw new ArgumentException("Template not found", nameof(templateId));

        return new ReportTemplateViewModel
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            ReportType = template.ReportType,
            Parameters = string.IsNullOrEmpty(template.Parameters)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(template.Parameters) ?? new Dictionary<string, object>(),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy,
            IsActive = template.IsActive,
            DefaultFormat = template.DefaultFormat,
            ScheduleFrequency = template.ScheduleFrequency,
            Recipients = string.IsNullOrEmpty(template.Recipients)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(template.Recipients) ?? new List<string>()
        };
    }

    public async Task<ReportTemplateViewModel> CreateReportTemplateAsync(CreateReportTemplateViewModel request)
    {
        var template = new ReportTemplate
        {
            Name = request.Name,
            Description = request.Description,
            ReportType = request.ReportType,
            Parameters = JsonSerializer.Serialize(request.Parameters),
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            DefaultFormat = request.DefaultFormat ?? "PDF",
            ScheduleFrequency = request.ScheduleFrequency,
            Recipients = JsonSerializer.Serialize(request.Recipients)
        };

        _db.ReportTemplates.Add(template);
        await _db.SaveChangesAsync();

        return await GetReportTemplateAsync(template.Id);
    }

    public async Task<bool> UpdateReportTemplateAsync(int templateId, UpdateReportTemplateViewModel request)
    {
        var template = await _db.ReportTemplates.FindAsync(templateId);
        if (template == null) return false;

        template.Name = request.Name;
        template.Description = request.Description;
        template.Parameters = JsonSerializer.Serialize(request.Parameters);
        template.UpdatedAt = DateTime.UtcNow;
        template.IsActive = request.IsActive;
        template.DefaultFormat = request.DefaultFormat;
        template.ScheduleFrequency = request.ScheduleFrequency;
        template.Recipients = JsonSerializer.Serialize(request.Recipients);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReportTemplateAsync(int templateId)
    {
        var template = await _db.ReportTemplates.FindAsync(templateId);
        if (template == null) return false;

        _db.ReportTemplates.Remove(template);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ScheduledReportViewModel>> GetScheduledReportsAsync()
    {
        return await _db.ScheduledReports
            .Where(sr => sr.IsActive)
            .Include(sr => sr.Template)
            .Select(sr => new ScheduledReportViewModel
            {
                Id = sr.Id,
                TemplateId = sr.TemplateId,
                TemplateName = sr.Template.Name,
                ScheduleFrequency = sr.ScheduleFrequency,
                NextRun = sr.NextRun,
                LastRun = sr.LastRun,
                IsActive = sr.IsActive,
                Recipients = string.IsNullOrEmpty(sr.Recipients)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(sr.Recipients, (JsonSerializerOptions?)null) ?? new List<string>(),
                Parameters = string.IsNullOrEmpty(sr.Parameters)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(sr.Parameters, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>(),
                CreatedAt = sr.CreatedAt,
                UpdatedAt = sr.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ScheduledReportViewModel> GetScheduledReportAsync(int reportId)
    {
        var report = await _db.ScheduledReports
            .Include(sr => sr.Template)
            .FirstOrDefaultAsync(sr => sr.Id == reportId);
        
        if (report == null)
            throw new ArgumentException("Scheduled report not found", nameof(reportId));

        return new ScheduledReportViewModel
        {
            Id = report.Id,
            TemplateId = report.TemplateId,
            TemplateName = report.Template.Name,
            ScheduleFrequency = report.ScheduleFrequency,
            NextRun = report.NextRun,
            LastRun = report.LastRun,
            IsActive = report.IsActive,
            Recipients = string.IsNullOrEmpty(report.Recipients)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(report.Recipients) ?? new List<string>(),
            Parameters = string.IsNullOrEmpty(report.Parameters)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(report.Parameters) ?? new Dictionary<string, object>(),
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };
    }

    public async Task<ScheduledReportViewModel> CreateScheduledReportAsync(CreateScheduledReportViewModel request)
    {
        var scheduledReport = new ScheduledReport
        {
            TemplateId = request.TemplateId,
            ScheduleFrequency = request.ScheduleFrequency,
            NextRun = CalculateNextRun(request.ScheduleFrequency),
            IsActive = true,
            Recipients = JsonSerializer.Serialize(request.Recipients),
            Parameters = JsonSerializer.Serialize(request.Parameters),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.ScheduledReports.Add(scheduledReport);
        await _db.SaveChangesAsync();

        return await GetScheduledReportAsync(scheduledReport.Id);
    }

    public async Task<bool> UpdateScheduledReportAsync(int reportId, UpdateScheduledReportViewModel request)
    {
        var report = await _db.ScheduledReports.FindAsync(reportId);
        if (report == null) return false;

        report.ScheduleFrequency = request.ScheduleFrequency;
        report.NextRun = CalculateNextRun(request.ScheduleFrequency);
        report.IsActive = request.IsActive;
        report.Recipients = JsonSerializer.Serialize(request.Recipients);
        report.Parameters = JsonSerializer.Serialize(request.Parameters);
        report.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteScheduledReportAsync(int reportId)
    {
        var report = await _db.ScheduledReports.FindAsync(reportId);
        if (report == null) return false;

        _db.ScheduledReports.Remove(report);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExecuteScheduledReportAsync(int reportId)
    {
        var report = await _db.ScheduledReports.FindAsync(reportId);
        if (report == null) return false;

        // Execute the report generation
        var reportData = new Dictionary<string, object>
        {
            ["ExecutedAt"] = DateTime.UtcNow,
            ["Parameters"] = JsonSerializer.Deserialize<Dictionary<string, object>>(report.Parameters ?? "{}")
                ?? new Dictionary<string, object>()
        };

        var history = new ReportHistory
        {
            ReportName = $"Scheduled Report {reportId}",
            ReportType = "Scheduled",
            GeneratedBy = "System",
            GeneratedAt = DateTime.UtcNow,
            Status = "Completed",
            Format = "PDF"
        };

        _db.ReportHistories.Add(history);

        report.LastRun = DateTime.UtcNow;
        report.NextRun = CalculateNextRun(report.ScheduleFrequency);
        report.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ReportHistoryViewModel>> GetReportHistoryAsync(string? userId = null, int count = 50)
    {
        var query = _db.ReportHistories.AsQueryable();
        
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(rh => rh.GeneratedBy == userId);

        return await query
            .OrderByDescending(rh => rh.GeneratedAt)
            .Take(count)
            .Select(rh => new ReportHistoryViewModel
            {
                Id = rh.Id,
                ReportId = rh.Id.ToString(),
                ReportName = rh.ReportName,
                ReportType = rh.ReportType,
                GeneratedBy = rh.GeneratedBy,
                GeneratedAt = rh.GeneratedAt,
                FileSize = rh.FileSize,
                Format = rh.Format,
                Status = rh.Status,
                ErrorMessage = rh.ErrorMessage ?? string.Empty,
                DownloadUrl = rh.DownloadUrl ?? string.Empty,
                ExpiresAt = rh.ExpiresAt
            })
            .ToListAsync();
    }

    public async Task<ReportHistoryViewModel> GetReportHistoryAsync(int historyId)
    {
        var history = await _db.ReportHistories.FindAsync(historyId);
        if (history == null)
            throw new ArgumentException("Report history not found", nameof(historyId));

        return new ReportHistoryViewModel
        {
            Id = history.Id,
            ReportId = history.Id.ToString(),
            ReportName = history.ReportName,
            ReportType = history.ReportType,
            GeneratedBy = history.GeneratedBy,
            GeneratedAt = history.GeneratedAt,
            FileSize = history.FileSize,
            Format = history.Format,
            Status = history.Status,
            ErrorMessage = history.ErrorMessage ?? string.Empty,
            DownloadUrl = history.DownloadUrl ?? string.Empty,
            ExpiresAt = history.ExpiresAt
        };
    }

    public async Task<byte[]> ExportReportAsync(ReportGenerationViewModel report, string format)
    {
        // Placeholder for actual report export logic
        // This would typically use libraries like EPPlus or QuestPDF
        return Array.Empty<byte>();
    }

    public async Task<ReportPreviewViewModel> PreviewReportAsync(ReportRequestViewModel request)
    {
        // Placeholder for report preview logic
        return new ReportPreviewViewModel
        {
            ReportType = request.ReportType,
            PreviewData = new List<object>(),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<ReportStatisticsViewModel> GetReportStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _db.ReportHistories.AsQueryable();
        
        if (fromDate.HasValue)
            query = query.Where(rh => rh.GeneratedAt >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(rh => rh.GeneratedAt <= toDate.Value);

        var totalReports = await query.CountAsync();
        var successfulReports = await query.CountAsync(rh => rh.Status == "Completed");
        var failedReports = await query.CountAsync(rh => rh.Status == "Failed");

        return new ReportStatisticsViewModel
        {
            TotalReports = totalReports,
            SuccessfulReports = successfulReports,
            FailedReports = failedReports
        };
    }

    public async Task<List<CustomFieldViewModel>> GetCustomFieldsAsync(string reportType)
    {
        return await _db.CustomReportFields
            .Where(f => f.ReportType == reportType && f.IsActive)
            .Select(f => new CustomFieldViewModel
            {
                Id = f.Id,
                Name = f.Name,
                Label = f.Label,
                Type = f.Type,
                Required = f.Required,
                DefaultValue = f.DefaultValue ?? string.Empty,
                Options = string.IsNullOrEmpty(f.Options)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(f.Options, (JsonSerializerOptions?)null) ?? new List<string>(),
                Order = f.Order
            })
            .OrderBy(f => f.Order)
            .ToListAsync();
    }

    public async Task<bool> SaveCustomFieldsAsync(string reportType, List<CustomFieldViewModel> fields)
    {
        var existingFields = await _db.CustomReportFields
            .Where(f => f.ReportType == reportType)
            .ToListAsync();

        _db.CustomReportFields.RemoveRange(existingFields);

        foreach (var field in fields)
        {
            _db.CustomReportFields.Add(new CustomReportField
            {
                Name = field.Name,
                Label = field.Label,
                Type = field.Type,
                Required = field.Required,
                DefaultValue = field.DefaultValue,
                Options = JsonSerializer.Serialize(field.Options),
                Order = field.Order,
                ReportType = reportType,
                IsActive = true
            });
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public Task<ReportValidationViewModel> ValidateReportRequestAsync(ReportRequestViewModel request)
    {
        var validation = new ReportValidationViewModel
        {
            IsValid = true,
            Errors = new List<string>(),
            Warnings = new List<string>()
        };

        if (string.IsNullOrEmpty(request.ReportType))
        {
            validation.IsValid = false;
            validation.Errors.Add("Report type is required");
        }

        return Task.FromResult(validation);
    }

    public async Task<ReportPermissionViewModel> GetReportPermissionsAsync(string userId)
    {
        var permissions = await _db.ReportPermissions
            .Where(rp => rp.UserId == userId)
            .ToListAsync();

        return new ReportPermissionViewModel
        {
            UserId = userId,
            Permissions = new Dictionary<string, List<string>>()
        };
    }

    public async Task<bool> GrantReportPermissionAsync(string userId, string reportType, string permission)
    {
        var existing = await _db.ReportPermissions
            .FirstOrDefaultAsync(rp => rp.UserId == userId && rp.ReportType == reportType);

        if (existing != null)
        {
            existing.Permission = permission;
            existing.GrantedAt = DateTime.UtcNow;
        }
        else
        {
            _db.ReportPermissions.Add(new ReportPermission
            {
                UserId = userId,
                ReportType = reportType,
                Permission = permission,
                GrantedAt = DateTime.UtcNow,
                TenantId = 1
            });
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeReportPermissionAsync(string userId, string reportType, string permission)
    {
        var existing = await _db.ReportPermissions
            .FirstOrDefaultAsync(rp => rp.UserId == userId && rp.ReportType == reportType);

        if (existing == null) return false;

        _db.ReportPermissions.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }

    private DateTime CalculateNextRun(string frequency)
    {
        return frequency.ToLower() switch
        {
            "daily" => DateTime.UtcNow.AddDays(1),
            "weekly" => DateTime.UtcNow.AddDays(7),
            "monthly" => DateTime.UtcNow.AddMonths(1),
            _ => DateTime.UtcNow.AddDays(1)
        };
    }
}
