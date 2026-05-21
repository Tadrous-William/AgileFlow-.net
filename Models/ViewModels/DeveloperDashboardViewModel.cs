namespace AgileTaskManager.Models.ViewModels;

public class DeveloperDashboardViewModel
{
    // Personal Overview
    public string UserName { get; set; } = string.Empty;

    // Task Statistics
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    
    // Performance Metrics
    public double AverageCompletionTime { get; set; }
    public int TasksCompletedThisWeek { get; set; }
    public int TasksCompletedThisMonth { get; set; }
    public double ProductivityScore { get; set; }
    public List<TaskPerformanceViewModel> RecentTaskPerformance { get; set; } = new();
    
    // Current Workload
    public List<TaskListViewModel> AssignedTasks { get; set; } = new();
    public List<TaskListViewModel> OverdueTasks { get; set; } = new();
    public List<TaskListViewModel> BlockedTasks { get; set; } = new();
    public List<TaskListViewModel> UpcomingDeadlines { get; set; } = new();
    
    // Sprint Information
    public List<SprintSummaryViewModel> ActiveSprints { get; set; } = new();
    public SprintSummaryViewModel? CurrentSprint { get; set; }
    public List<TaskListViewModel> SprintTasks { get; set; } = new();
    
    // Project Involvement
    public List<ProjectSummaryViewModel> Projects { get; set; } = new();
    public int ActiveProjects { get; set; }

    // Time Tracking
    public List<TimeEntryViewModel> RecentTimeEntries { get; set; } = new();
    public TimeSpan TotalTimeToday { get; set; }
    public TimeSpan TotalTimeThisWeek { get; set; }
    public TimeSpan AverageDailyTime { get; set; }
    
    // Meetings
    public List<MeetingSummaryViewModel> UpcomingMeetings { get; set; } = new();
    public List<MeetingSummaryViewModel> RecentMeetings { get; set; } = new();
    
    // Recent Activity
    public List<ActivityLogViewModel> RecentActivity { get; set; } = new();
    public List<CommentViewModel> RecentComments { get; set; } = new();
    
    public DateTime LastDataUpdate { get; set; } = DateTime.UtcNow;
}

public class TaskPerformanceViewModel
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? CompletionTime { get; set; }
    public string Priority { get; set; } = string.Empty;
    public bool WasOverdue { get; set; }
}
