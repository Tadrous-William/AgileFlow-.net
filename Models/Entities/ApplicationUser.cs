using Microsoft.AspNetCore.Identity;
using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class ApplicationUser : IdentityUser, ITenantEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string Role { get; set; } = "Developer";   // Admin | TeamLead | Developer | Viewer
    
    // Security properties
    public DateTime? LastLoginDate { get; set; }
    public bool PasswordChangeRequired { get; set; } = false;
    public DateTime? PasswordChangeDate { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Navigation collections
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    
    // Gamification
    public UserProfile? UserProfile { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public int TotalXP { get; set; } = 0;
}
