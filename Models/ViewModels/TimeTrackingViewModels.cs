using AgileTaskManager.Models.Entities;

namespace AgileTaskManager.Models.ViewModels;

public class TimeEntryViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string DurationFormatted => TimeFormattingHelper.FormatDuration(Duration);
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ProjectName { get; set; }
}

public class CreateTimeEntryViewModel
{
    public int TaskId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Description { get; set; }
}

public class UpdateTimeEntryViewModel
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Description { get; set; }
}

public class TimeSheetViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan TotalTime { get; set; }
    public string TotalTimeFormatted => TimeFormattingHelper.FormatDuration(TotalTime);
    public TimeSpan BillableTime { get; set; }
    public string BillableTimeFormatted => TimeFormattingHelper.FormatDuration(BillableTime);
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public TimeSheetStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public List<TimeEntryViewModel> TimeEntries { get; set; } = new();
    public int EntryCount => TimeEntries.Count;
}

public class TimeTrackingDashboardViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    // Today's stats
    public TimeSpan TodayTotalTime { get; set; }
    public string TodayTotalFormatted => TimeFormattingHelper.FormatDuration(TodayTotalTime);
    public int TodayEntryCount { get; set; }
    public bool HasActiveTimer { get; set; }
    public TimeEntryViewModel? ActiveTimer { get; set; }
    
    // Week stats
    public TimeSpan WeekTotalTime { get; set; }
    public string WeekTotalFormatted => TimeFormattingHelper.FormatDuration(WeekTotalTime);
    public int WeekEntryCount { get; set; }
    
    // Month stats
    public TimeSpan MonthTotalTime { get; set; }
    public string MonthTotalFormatted => TimeFormattingHelper.FormatDuration(MonthTotalTime);
    public int MonthEntryCount { get; set; }
    
    // Recent entries
    public List<TimeEntryViewModel> RecentEntries { get; set; } = new();
    
    // Task breakdown
    public List<TaskTimeSummaryViewModel> TaskBreakdown { get; set; } = new();
    
    // Daily breakdown (last 7 days)
    public List<DailyTimeSummaryViewModel> DailyBreakdown { get; set; } = new();
}

public class TaskTimeSummaryViewModel
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public TimeSpan TotalTime { get; set; }
    public string TotalTimeFormatted => TimeFormattingHelper.FormatDuration(TotalTime);
    public int EntryCount { get; set; }
    public string? ProjectName { get; set; }
}

public class DailyTimeSummaryViewModel
{
    public DateTime Date { get; set; }
    public TimeSpan TotalTime { get; set; }
    public string TotalTimeFormatted => TimeFormattingHelper.FormatDuration(TotalTime);
    public int EntryCount { get; set; }
    public bool IsToday { get; set; }
}

public class TimeTrackingSettingsViewModel
{
    public bool EnableTimeTracking { get; set; } = true;
    public bool RequireDescription { get; set; } = false;
    public bool AllowManualTimeEntry { get; set; } = true;
    public int MinimumEntryMinutes { get; set; } = 1;
    public int MaximumHoursPerDay { get; set; } = 24;
    public decimal DefaultHourlyRate { get; set; } = 0;
    public string TimeZone { get; set; } = "UTC";
}

public class TimerControlViewModel
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public DateTime? StartTime { get; set; }
    public TimeSpan? ElapsedTime { get; set; }
    public string ElapsedTimeFormatted => ElapsedTime.HasValue ? TimeFormattingHelper.FormatDuration(ElapsedTime.Value) : "00:00:00";
    public string? Description { get; set; }
}

public class TimeReportViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedByUserName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReportType Type { get; set; }
    public ReportScope Scope { get; set; }
    public string? ProjectName { get; set; }
    public string? UserName { get; set; }
    public TimeSpan TotalHours { get; set; }
    public string TotalHoursFormatted => TimeFormattingHelper.FormatDuration(TotalHours);
    public decimal TotalAmount { get; set; }
    public bool IsPublic { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? FilePath { get; set; }
}

public class CreateTimeReportViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReportType Type { get; set; }
    public ReportScope Scope { get; set; }
    public int? ProjectId { get; set; }
    public string? UserId { get; set; }
    public bool IsPublic { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
}

public class TimeAnalyticsViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan TotalTime { get; set; }
    public string TotalTimeFormatted => TimeFormattingHelper.FormatDuration(TotalTime);
    public decimal TotalAmount { get; set; }
    public int TotalEntries { get; set; }
    public TimeSpan AverageDailyTime { get; set; }
    public string AverageDailyTimeFormatted => TimeFormattingHelper.FormatDuration(AverageDailyTime);
    
    // Breakdown by user
    public List<UserTimeAnalyticsViewModel> UserBreakdown { get; set; } = new();
    
    // Breakdown by project
    public List<ProjectTimeAnalyticsViewModel> ProjectBreakdown { get; set; } = new();
    
    // Breakdown by task
    public List<TaskTimeAnalyticsViewModel> TaskBreakdown { get; set; } = new();
    
    // Daily trend
    public List<DailyTimeSummaryViewModel> DailyTrend { get; set; } = new();
}

public class UserTimeAnalyticsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public TimeSpan TotalTime { get; set; }
    public string TotalTimeFormatted => TimeFormattingHelper.FormatDuration(TotalTime);
    public decimal TotalAmount { get; set; }
    public int EntryCount { get; set; }
    public TimeSpan AverageTimePerEntry { get; set; }
}

public class ProjectTimeAnalyticsViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public TimeSpan TotalTime { get; set; }
    public string TotalTimeFormatted => TimeFormattingHelper.FormatDuration(TotalTime);
    public decimal TotalAmount { get; set; }
    public int EntryCount { get; set; }
    public int UserCount { get; set; }
}

public class TaskTimeAnalyticsViewModel
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public TimeSpan TotalTime { get; set; }
    public string TotalTimeFormatted => TimeFormattingHelper.FormatDuration(TotalTime);
    public int EntryCount { get; set; }
    public int UserCount { get; set; }
    public TimeSpan AverageTimePerUser { get; set; }
}

// Helper method for formatting duration
public static class TimeFormattingHelper
{
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
            return $"{(int)duration.TotalDays}d {duration.Hours:D2}h {duration.Minutes:D2}m";
        
        if (duration.TotalHours >= 1)
            return $"{duration.Hours:D2}h {duration.Minutes:D2}m";
        
        return $"{duration.Minutes:D2}m {duration.Seconds:D2}s";
    }
}
