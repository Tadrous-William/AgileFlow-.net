using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class UserProfile : ITenantEntity
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Gamification properties
    public int TotalXP { get; set; } = 0;
    public int CurrentLevel { get; set; } = 1;
    public int XPToNextLevel { get; set; } = 100;
    public int TasksCompleted { get; set; } = 0;
    public int TasksCreated { get; set; } = 0;
    public int CommentsPosted { get; set; } = 0;
    public int StreakDays { get; set; } = 0;
    public int TasksAssigned { get; set; } = 0;
    public DateTime LastActivityDate { get; set; } = DateTime.UtcNow;
    
    // Navigation collections
    public ICollection<UserBadge> Badges { get; set; } = new List<UserBadge>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}

public class Badge : ITenantEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; // CSS class or emoji
    public string Category { get; set; } = string.Empty; // e.g., "Tasks", "Collaboration", "Streak"
    public int XPReward { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Badge criteria (JSON string for flexibility)
    public string Criteria { get; set; } = string.Empty;
    
    // Navigation collections
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}

public class UserBadge : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public int BadgeId { get; set; }
    public Badge Badge { get; set; } = null!;
    
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    public int XPAwarded { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class XPHistory : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public int XPAmount { get; set; }
    public string Reason { get; set; } = string.Empty; // e.g., "Task Completed", "Badge Earned", "Comment Posted"
    public string? SourceId { get; set; } // e.g., Task ID, Badge ID
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    
    // Level information at time of award
    public int LevelAtTime { get; set; }
    public bool WasLevelUp { get; set; } = false;
}

public class TeamLeaderboard : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public int TotalXP { get; set; }
    public int Rank { get; set; }
    public int TasksCompleted { get; set; }
    public int BadgesEarned { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class LevelDefinition
{
    public int Level { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RequiredXP { get; set; }
    public string BadgeIcon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty; // CSS color class
}
