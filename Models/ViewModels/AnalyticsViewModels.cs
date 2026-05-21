using AgileTaskManager.Models.Enums;
using System;
using System.Collections.Generic;

namespace AgileTaskManager.Models.ViewModels;

public class AnalyticsOverviewViewModel
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }
    public int TotalProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double CompletionRate { get; set; }
    public double UserGrowthRate { get; set; }
    public double ProjectGrowthRate { get; set; }
}

public class UserAnalyticsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public double AverageCompletionTime { get; set; }
    public double ProductivityScore { get; set; }
}

public class GamificationAnalyticsViewModel
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public double AverageLevel { get; set; }
    public int TotalXPAwarded { get; set; }
    public int TotalBadgesEarned { get; set; }
    public List<LeaderboardEntryViewModel> TopPerformers { get; set; } = new();
    public Dictionary<string, int> EngagementMetrics { get; set; } = new();
}

public class SystemHealthAnalyticsViewModel
{
    public DateTime Timestamp { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public double UserActivityRate { get; set; }
    public double TaskCompletionRate { get; set; }
    public TimeSpan SystemUptime { get; set; }
    public double ErrorRate { get; set; }
    public double AverageResponseTime { get; set; }
}

public class PerformanceAnalyticsViewModel
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public double AverageResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
    public double P99ResponseTime { get; set; }
    public int TotalRequests { get; set; }
    public double ErrorRate { get; set; }
    public List<double> TopSlowestEndpoints { get; set; } = new();
    public List<KeyValuePair<string, int>> MostCommonErrors { get; set; } = new();
}

public class UsageAnalyticsViewModel
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalUsers { get; set; }
    public Dictionary<DateTime, int> DailyActiveUsers { get; set; } = new();
    public List<int> PeakUsageHours { get; set; } = new();
    public double AverageSessionDuration { get; set; }
    public Dictionary<string, int> FeatureUsage { get; set; } = new();
}

public class SecurityAnalyticsViewModel
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalSecurityEvents { get; set; }
    public int FailedLogins { get; set; }
    public int SuccessfulLogins { get; set; }
    public int PasswordChangeEvents { get; set; }
    public int AccountLockEvents { get; set; }
    public int SuspiciousActivities { get; set; }
    public Dictionary<string, int> SecurityEventsByType { get; set; } = new();
    public List<KeyValuePair<string, int>> TopRiskFactors { get; set; } = new();
}

public class TopPagesViewModel
{
    public string Page { get; set; } = string.Empty;
    public int Views { get; set; }
    public int UniqueUsers { get; set; }
}

public class ActivityHeatmapViewModel
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public Dictionary<int, int> HourlyActivity { get; set; } = new();
    public Dictionary<DayOfWeek, int> DayOfWeekActivity { get; set; } = new();
}


public class BurndownPointViewModel
{
    public DateTime Date { get; set; }
    public int IdealRemaining { get; set; }
    public int ActualRemaining { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
}

public class TaskAnalyticsViewModel
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? CompletionTime { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    
    // Additional properties for analytics
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public double AverageCompletionTime { get; set; }
    public List<object> TaskCreationTrend { get; set; } = new();
}

public class SprintAnalyticsViewModel
{
    public int SprintId { get; set; }
    public string SprintName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; }
    
    // Task Statistics
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int ToDoTasks { get; set; }
    public int TestingTasks { get; set; }
    public int OverdueTasks { get; set; }
    
    // Progress Metrics
    public double CompletionPercentage { get; set; } = 0;
    public int BlockedTasks { get; set; }
    public double AverageTaskCompletionTime { get; set; }
    public int RemainingDays => Math.Max(0, (EndDate - DateTime.UtcNow).Days);
    public int TotalDays => (EndDate - StartDate).Days;
    public double SprintProgressPercentage => TotalDays > 0 ? (double)(TotalDays - RemainingDays) / TotalDays * 100 : 0;
    
    // Burndown Data
    public List<BurndownPointViewModel> BurndownData { get; set; } = new();
    
    // Task Distribution
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    
    // Team Performance
    public List<TeamMemberPerformanceViewModel> TeamPerformance { get; set; } = new();
    
    // Recent Activity
    public List<ActivityLogViewModel> RecentActivity { get; set; } = new();
}

public class TeamMemberPerformanceViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int AssignedTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double CompletionRate => AssignedTasks > 0 ? (double)CompletedTasks / AssignedTasks * 100 : 0;
    public int OverdueTasks { get; set; }
}

public class ProjectAnalyticsViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    
    // Overall Project Stats
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ActiveSprintsCount { get; set; }
    public int TotalSprintsCount { get; set; }
    public int TeamSize { get; set; }
    
    // Sprint Performance
    public List<SprintSummaryViewModel> SprintSummaries { get; set; } = new();
    
    // Task Trends (last 30 days)
    public List<TaskTrendViewModel> TaskTrends { get; set; } = new();
    
    // Additional properties for analytics
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public decimal BudgetUtilization { get; set; }
    public double ProjectVelocity { get; set; }
}

public class SprintSummaryViewModel
{
    public int SprintId { get; set; }
    public string SprintName { get; set; } = string.Empty;
    public SprintStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public double CompletionPercentage => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks * 100 : 0;
}

public class TaskTrendViewModel
{
    public DateTime Date { get; set; }
    public int CreatedTasks { get; set; }
    public int CompletedTasks { get; set; }
}
