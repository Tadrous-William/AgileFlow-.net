using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AgileTaskManager.Services;

public class TimeTrackingService : ITimeTrackingService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TimeTrackingService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetClientIP()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null) return "127.0.0.1";
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrEmpty(forwarded) ? forwarded : ctx.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    private string GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.FirstOrDefault() ?? "AgileTaskManager";
    }

    public async Task<TimeEntryViewModel> StartTimerAsync(int taskId, string userId, string? description = null)
    {
        // Check if user already has an active timer
        var existingActive = await _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
            .FirstOrDefaultAsync(te => te.UserId == userId && te.EndTime == null);

        if (existingActive != null)
        {
            // Stop the existing timer first
            existingActive.EndTime = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // Validate task exists
        var task = await _db.Tasks.FindAsync(taskId);
        if (task == null)
            throw new ArgumentException("Task not found", nameof(taskId));

        // Create new time entry
        var timeEntry = new TimeEntry
        {
            TenantId = 1,          // single-tenant app; TenantId required by FK constraint
            UserId = userId,
            TaskId = taskId,
            StartTime = DateTime.UtcNow,
            Description = description,
            IPAddress = GetClientIP(),
            UserAgent = GetUserAgent()
        };

        _db.TimeEntries.Add(timeEntry);
        await _db.SaveChangesAsync();

        // Reload with navigation properties for the view model
        timeEntry = await _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .FirstAsync(te => te.Id == timeEntry.Id);

        return await MapToViewModelAsync(timeEntry);
    }

    public async Task<TimeEntryViewModel> StopTimerAsync(int taskId, string userId)
    {
        var activeEntry = await _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(te => te.UserId == userId && te.TaskId == taskId && te.EndTime == null);

        if (activeEntry == null)
            throw new ArgumentException("No active timer found for this task", nameof(taskId));

        activeEntry.EndTime = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await MapToViewModelAsync(activeEntry);
    }

    public async Task<TimeEntryViewModel> GetActiveTimerAsync(string userId)
    {
        var activeEntry = await _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(te => te.UserId == userId && te.EndTime == null);

        if (activeEntry == null)
            throw new ArgumentException("No active timer found", nameof(userId));

        return await MapToViewModelAsync(activeEntry);
    }

    public async Task<List<TimeEntryViewModel>> GetUserTimeEntriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .Where(te => te.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(te => te.StartTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(te => te.StartTime <= endDate.Value);

        var entries = await query
            .OrderByDescending(te => te.StartTime)
            .ToListAsync();

        var viewModels = new List<TimeEntryViewModel>();
        foreach (var entry in entries)
        {
            viewModels.Add(await MapToViewModelAsync(entry));
        }

        return viewModels;
    }

    public async Task<TimeEntryViewModel> CreateManualTimeEntryAsync(CreateTimeEntryViewModel model, string userId)
    {
        // Validate task exists
        var task = await _db.Tasks.FindAsync(model.TaskId);
        if (task == null)
            throw new ArgumentException("Task not found", nameof(model.TaskId));

        // Validate time range
        if (model.EndTime.HasValue && model.EndTime.Value <= model.StartTime)
            throw new ArgumentException("End time must be after start time");

        var timeEntry = new TimeEntry
        {
            TenantId = 1,          // single-tenant app
            UserId = userId,
            TaskId = model.TaskId,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Description = model.Description,
            IPAddress = GetClientIP(),
            UserAgent = GetUserAgent()
        };

        _db.TimeEntries.Add(timeEntry);
        await _db.SaveChangesAsync();

        // Reload with navigation properties for the view model
        timeEntry = await _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .FirstAsync(te => te.Id == timeEntry.Id);

        return await MapToViewModelAsync(timeEntry);
    }

    public async Task<TimeEntryViewModel> UpdateTimeEntryAsync(UpdateTimeEntryViewModel model, string userId)
    {
        var entry = await _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .FirstOrDefaultAsync(te => te.Id == model.Id && te.UserId == userId);

        if (entry == null)
            throw new ArgumentException("Time entry not found", nameof(model.Id));

        // Validate time range
        if (model.EndTime.HasValue && model.EndTime.Value <= model.StartTime)
            throw new ArgumentException("End time must be after start time");

        entry.StartTime = model.StartTime;
        entry.EndTime = model.EndTime;
        entry.Description = model.Description;

        await _db.SaveChangesAsync();

        return await MapToViewModelAsync(entry);
    }

    public async Task<bool> DeleteTimeEntryAsync(int entryId, string userId)
    {
        var entry = await _db.TimeEntries
            .FirstOrDefaultAsync(te => te.Id == entryId && te.UserId == userId);

        if (entry == null)
            return false;

        _db.TimeEntries.Remove(entry);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<TimeTrackingDashboardViewModel> GetDashboardAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);

        // Get all entries for the user
        var allEntries = await _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .Where(te => te.UserId == userId)
            .ToListAsync();

        // Today's entries
        var todayEntries = allEntries.Where(te => te.StartTime.Date == today).ToList();
        var todayTotal = todayEntries.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration);

        // Week entries
        var weekEntries = allEntries.Where(te => te.StartTime.Date >= weekStart).ToList();
        var weekTotal = weekEntries.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration);

        // Month entries
        var monthEntries = allEntries.Where(te => te.StartTime.Date >= monthStart).ToList();
        var monthTotal = monthEntries.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration);

        // Active timer
        var activeTimer = allEntries.FirstOrDefault(te => te.EndTime == null);
        var activeTimerViewModel = activeTimer != null ? await MapToViewModelAsync(activeTimer) : null;

        // Recent entries (last 10)
        var recentEntries = allEntries
            .OrderByDescending(te => te.StartTime)
            .Take(10)
            .ToList();

        var recentViewModels = new List<TimeEntryViewModel>();
        foreach (var entry in recentEntries)
        {
            recentViewModels.Add(await MapToViewModelAsync(entry));
        }

        // Task breakdown
        var taskBreakdown = allEntries
            .Where(te => te.Duration.TotalMinutes > 0)
            .GroupBy(te => te.TaskId)
            .Select(g => new TaskTimeSummaryViewModel
            {
                TaskId = g.Key,
                TaskTitle = g.First().Task.Title,
                TotalTime = g.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration),
                EntryCount = g.Count(),
                ProjectName = g.First().Task.Project?.Name
            })
            .OrderByDescending(t => t.TotalTime)
            .Take(10)
            .ToList();

        // Daily breakdown (last 7 days)
        var dailyBreakdown = new List<DailyTimeSummaryViewModel>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayEntries = allEntries.Where(te => te.StartTime.Date == date).ToList();
            var dayTotal = dayEntries.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration);

            dailyBreakdown.Add(new DailyTimeSummaryViewModel
            {
                Date = date,
                TotalTime = dayTotal,
                EntryCount = dayEntries.Count,
                IsToday = date == today
            });
        }

        return new TimeTrackingDashboardViewModel
        {
            UserId = userId,
            UserName = allEntries.FirstOrDefault()?.User?.FullName ?? "",
            TodayTotalTime = todayTotal,
            TodayEntryCount = todayEntries.Count,
            HasActiveTimer = activeTimer != null,
            ActiveTimer = activeTimerViewModel,
            WeekTotalTime = weekTotal,
            WeekEntryCount = weekEntries.Count,
            MonthTotalTime = monthTotal,
            MonthEntryCount = monthEntries.Count,
            RecentEntries = recentViewModels,
            TaskBreakdown = taskBreakdown,
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<TimeSheetViewModel> GetTimeSheetAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var entries = await _db.TimeEntries
            .Include(te => te.User)
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .Where(te => te.UserId == userId && te.StartTime.Date >= startDate.Date && te.StartTime.Date <= endDate.Date)
            .OrderBy(te => te.StartTime)
            .ToListAsync();

        var totalTime = entries.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration);
        var billableTime = totalTime; // Simplified - would need billable logic

        var entryViewModels = new List<TimeEntryViewModel>();
        foreach (var entry in entries)
        {
            entryViewModels.Add(await MapToViewModelAsync(entry));
        }

        return new TimeSheetViewModel
        {
            UserId = userId,
            UserName = entries.FirstOrDefault()?.User?.FullName ?? "",
            StartDate = startDate,
            EndDate = endDate,
            TotalTime = totalTime,
            BillableTime = billableTime,
            HourlyRate = 0, // Would get from user settings
            TotalAmount = 0,
            Status = TimeSheetStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            TimeEntries = entryViewModels
        };
    }

    public async Task<TimeAnalyticsViewModel> GetTimeAnalyticsAsync(DateTime startDate, DateTime endDate, int? projectId = null, string? userId = null)
    {
        var query = _db.TimeEntries
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .Include(te => te.User)
            .Where(te => te.StartTime.Date >= startDate.Date && te.StartTime.Date <= endDate.Date);

        if (projectId.HasValue)
            query = query.Where(te => te.Task.ProjectId == projectId.Value);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(te => te.UserId == userId);

        var entries = await query.ToListAsync();

        var totalTime = entries.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration);
        var totalAmount = 0m; // Would calculate based on hourly rates
        var totalEntries = entries.Count;
        var days = (endDate - startDate).Days + 1;
        var averageDailyTime = days > 0 ? TimeSpan.FromTicks(totalTime.Ticks / days) : TimeSpan.Zero;

        // User breakdown
        var userBreakdown = entries
            .GroupBy(te => te.UserId)
            .Select(g => new UserTimeAnalyticsViewModel
            {
                UserId = g.Key,
                UserName = g.First().User.FullName,
                TotalTime = g.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration),
                TotalAmount = 0m, // Would calculate based on hourly rates
                EntryCount = g.Count(),
                AverageTimePerEntry = g.Count() > 0 ? TimeSpan.FromTicks(g.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration).Ticks / g.Count()) : TimeSpan.Zero
            })
            .OrderByDescending(u => u.TotalTime)
            .ToList();

        // Project breakdown
        var projectBreakdown = entries
            .Where(te => te.Task.Project != null)
            .GroupBy(te => te.Task.ProjectId)
            .Select(g => new ProjectTimeAnalyticsViewModel
            {
                ProjectId = g.Key ?? 0,
                ProjectName = g.First().Task.Project?.Name ?? "Unknown",
                TotalTime = g.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration),
                TotalAmount = 0m,
                EntryCount = g.Count(),
                UserCount = g.Select(te => te.UserId).Distinct().Count()
            })
            .OrderByDescending(p => p.TotalTime)
            .ToList();

        // Task breakdown
        var taskBreakdown = entries
            .GroupBy(te => te.TaskId)
            .Select(g => new TaskTimeAnalyticsViewModel
            {
                TaskId = g.Key,
                TaskTitle = g.First().Task.Title,
                ProjectName = g.First().Task.Project?.Name,
                TotalTime = g.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration),
                EntryCount = g.Count(),
                UserCount = g.Select(te => te.UserId).Distinct().Count(),
                AverageTimePerUser = g.Select(te => te.UserId).Distinct().Count() > 0 
                    ? TimeSpan.FromTicks(g.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration).Ticks / g.Select(te => te.UserId).Distinct().Count()) 
                    : TimeSpan.Zero
            })
            .OrderByDescending(t => t.TotalTime)
            .Take(20)
            .ToList();

        // Daily trend
        var dailyTrend = new List<DailyTimeSummaryViewModel>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayEntries = entries.Where(te => te.StartTime.Date == date).ToList();
            var dayTotal = dayEntries.Aggregate(TimeSpan.Zero, (sum, te) => sum + te.Duration);

            dailyTrend.Add(new DailyTimeSummaryViewModel
            {
                Date = date,
                TotalTime = dayTotal,
                EntryCount = dayEntries.Count
            });
        }

        return new TimeAnalyticsViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalTime = totalTime,
            TotalAmount = totalAmount,
            TotalEntries = totalEntries,
            AverageDailyTime = averageDailyTime,
            UserBreakdown = userBreakdown,
            ProjectBreakdown = projectBreakdown,
            TaskBreakdown = taskBreakdown,
            DailyTrend = dailyTrend
        };
    }

    public async Task<TimeReportViewModel> GenerateTimeReportAsync(CreateTimeReportViewModel model, string generatedBy)
    {
        var analytics = await GetTimeAnalyticsAsync(model.StartDate, model.EndDate, model.ProjectId, model.UserId);

        var report = new TimeReport
        {
            Title = model.Title,
            Description = model.Description,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = generatedBy,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Type = model.Type,
            Scope = model.Scope,
            ProjectId = model.ProjectId,
            UserId = model.UserId,
            ReportData = System.Text.Json.JsonSerializer.Serialize(analytics),
            TotalHours = analytics.TotalTime,
            TotalAmount = analytics.TotalAmount,
            IsPublic = model.IsPublic,
            ExpiresAt = model.ExpiresAt
        };

        _db.TimeReports.Add(report);
        await _db.SaveChangesAsync();

        var generatedByUser = await _db.Users.FindAsync(generatedBy);
        var reportProject = model.ProjectId.HasValue ? await _db.Projects.FindAsync(model.ProjectId.Value) : null;
        var reportUser = model.UserId != null ? await _db.Users.FindAsync(model.UserId) : null;

        return new TimeReportViewModel
        {
            Id = report.Id,
            Title = report.Title,
            Description = report.Description,
            GeneratedAt = report.GeneratedAt,
            GeneratedByUserName = generatedByUser?.FullName ?? generatedBy,
            StartDate = report.StartDate,
            EndDate = report.EndDate,
            Type = report.Type,
            Scope = report.Scope,
            ProjectName = reportProject?.Name ?? "",
            UserName = reportUser?.FullName ?? "",
            TotalHours = report.TotalHours,
            TotalAmount = report.TotalAmount,
            IsPublic = report.IsPublic,
            ExpiresAt = report.ExpiresAt
        };
    }

    private async Task<TimeEntryViewModel> MapToViewModelAsync(TimeEntry entry)
    {
        // Ensure StartTime/EndTime are treated as UTC so JS receives "...Z" ISO strings
        // and new Date(startTime) computes the correct elapsed time.
        var startTimeUtc = DateTime.SpecifyKind(entry.StartTime, DateTimeKind.Utc);
        var endTimeUtc   = entry.EndTime.HasValue
            ? DateTime.SpecifyKind(entry.EndTime.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        return new TimeEntryViewModel
        {
            Id = entry.Id,
            UserId = entry.UserId,
            UserName = entry.User?.FullName ?? "Unknown",
            TaskId = entry.TaskId,
            TaskTitle = entry.Task?.Title ?? "Unknown Task",
            StartTime = startTimeUtc,
            EndTime = endTimeUtc,
            Duration = entry.Duration,
            Description = entry.Description,
            IsActive = entry.IsActive,
            CreatedAt = entry.CreatedAt,
            ProjectName = entry.Task?.Project?.Name
        };
    }
}
