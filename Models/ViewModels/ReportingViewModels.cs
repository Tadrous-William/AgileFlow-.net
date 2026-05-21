using System;
using System.Collections.Generic;

namespace AgileTaskManager.Models.ViewModels;

public class ReportRequestViewModel
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Format { get; set; } = "PDF";
    public string GeneratedBy { get; set; } = string.Empty;
}

public class ReportGenerationViewModel
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
}

public class ReportTemplateViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string DefaultFormat { get; set; } = string.Empty;
    public string ScheduleFrequency { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
}

public class CreateReportTemplateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public string DefaultFormat { get; set; } = "PDF";
    public string ScheduleFrequency { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
}

public class UpdateReportTemplateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsActive { get; set; }
    public string DefaultFormat { get; set; } = string.Empty;
    public string ScheduleFrequency { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
}

public class ScheduledReportViewModel
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string ScheduleFrequency { get; set; } = string.Empty;
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public bool IsActive { get; set; }
    public List<string> Recipients { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateScheduledReportViewModel
{
    public int TemplateId { get; set; }
    public string ScheduleFrequency { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class UpdateScheduledReportViewModel
{
    public string ScheduleFrequency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> Recipients { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class ReportHistoryViewModel
{
    public int Id { get; set; }
    public string ReportId { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string GeneratedBy { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public long FileSize { get; set; }
    public string Format { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}

public class ReportPreviewViewModel
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public List<object> PreviewData { get; set; } = new();
    public int TotalRecords { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class ReportStatisticsViewModel
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalReports { get; set; }
    public int SuccessfulReports { get; set; }
    public int FailedReports { get; set; }
    public Dictionary<string, int> ReportsByType { get; set; } = new();
    public Dictionary<string, int> ReportsByFormat { get; set; } = new();
    public double AverageFileSize { get; set; }
    public long TotalFileSize { get; set; }
    public Dictionary<string, int> TopReportTypes { get; set; } = new();
}

public class CustomFieldViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int Order { get; set; }
    public bool IsActive { get; set; }
}

public class ReportValidationViewModel
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ReportPermissionViewModel
{
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Permissions { get; set; } = new();
}
