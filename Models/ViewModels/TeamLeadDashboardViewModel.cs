namespace AgileTaskManager.Models.ViewModels;

public class TeamLeadDashboardViewModel
{
    // Team Overview
    public string TeamName { get; set; } = string.Empty;
    public int TeamSize { get; set; }
    public int ActiveMembersToday { get; set; }
    public List<TeamMemberViewModel> TeamMembers { get; set; } = new();
    
    // Project Statistics
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public List<ProjectSummaryViewModel> Projects { get; set; } = new();
    
    // Task Statistics
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    
    // Team Performance
    public List<TeamPerformanceViewModel> TeamPerformance { get; set; } = new();
    public List<LeaderboardEntryViewModel> TeamLeaderboard { get; set; } = new();
    public double TeamVelocity { get; set; }
    public double AverageTaskCompletionTime { get; set; }
    
    // Sprint Information
    public List<SprintSummaryViewModel> ActiveSprints { get; set; } = new();
    public SprintSummaryViewModel? CurrentSprint { get; set; }
    
    // Recent Activity
    public List<ActivityLogViewModel> RecentActivity { get; set; } = new();
    public List<TaskListViewModel> RecentTasks { get; set; } = new();
    public List<TaskListViewModel> BlockedTasks { get; set; } = new();
    
    // Meeting Schedule
    public List<MeetingSummaryViewModel> UpcomingMeetings { get; set; } = new();
    
    public DateTime LastDataUpdate { get; set; } = DateTime.UtcNow;
}

public class TeamMemberViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActiveToday { get; set; }
    public int TasksAssigned { get; set; }
    public int TasksCompleted { get; set; }
    public int CurrentStreak { get; set; }
    public DateTime LastActivity { get; set; }
}

public class ProjectSummaryViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double ProgressPercentage { get; set; }
    public int ActiveMembers { get; set; }
}

public class TeamPerformanceViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TasksCompleted { get; set; }
    public int TotalXP { get; set; }
    public int CurrentStreak { get; set; }
    public double AverageCompletionTime { get; set; }
    public int OverdueTasksCount { get; set; }
}


public class MeetingSummaryViewModel
{
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string Duration { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
}
