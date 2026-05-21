namespace AgileTaskManager.Models.ViewModels;

public class AdminDashboardViewModel
{
    // System Overview
    public int TotalUsers { get; set; }
    public int TotalProjects { get; set; }
    public int TotalTasks { get; set; }
    public int ActiveUsersToday { get; set; }
    public int TotalTenants { get; set; }
    
    // Task Statistics
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    
    // User Activity
    public List<UserActivityViewModel> TopPerformers { get; set; } = new();
    public List<UserActivityViewModel> RecentUsers { get; set; } = new();
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    
    // Gamification Stats
    public int TotalBadgesAwarded { get; set; }
    public int TotalXPAwarded { get; set; }
    public List<LeaderboardEntryViewModel> GlobalLeaderboard { get; set; } = new();
    
    // Recent Activity
    public List<ActivityLogViewModel> RecentActivity { get; set; } = new();
    public List<TaskListViewModel> RecentTasks { get; set; } = new();
    
    // System Health
    public List<SystemHealthViewModel> SystemHealth { get; set; } = new();
    public DateTime LastDataUpdate { get; set; } = DateTime.UtcNow;
}

public class UserActivityViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int TasksCompleted { get; set; }
    public int TotalXP { get; set; }
    public int CurrentStreak { get; set; }
    public DateTime LastActivity { get; set; }
}

public class SystemHealthViewModel
{
    public string Metric { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Healthy, Warning, Critical
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
