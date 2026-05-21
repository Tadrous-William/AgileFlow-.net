using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class Meeting : ITenantEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MeetingType Type { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public TimeSpan? Duration => StartedAt.HasValue && EndedAt.HasValue ? EndedAt.Value - StartedAt.Value : null;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public int? SprintId { get; set; }
    public Sprint? Sprint { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    
    public string? FacilitatedBy { get; set; }
    public ApplicationUser? Facilitator { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation collections
    public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    public ICollection<MeetingNote> Notes { get; set; } = new List<MeetingNote>();
    public ICollection<MeetingActionItem> ActionItems { get; set; } = new List<MeetingActionItem>();
}

public enum MeetingType
{
    DailyStandup,
    SprintPlanning,
    SprintReview,
    SprintRetrospective,
    TeamMeeting,
    OneOnOne,
    ClientMeeting,
    Other
}

public enum MeetingStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Postponed
}

public class MeetingParticipant : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Invited;
    public DateTime? RespondedAt { get; set; }
    public string? ResponseNote { get; set; }
    public bool IsFacilitator { get; set; } = false;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

public enum ParticipantStatus
{
    Invited,
    Accepted,
    Declined,
    Tentative,
    NoShow
}

public class MeetingNote : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string Content { get; set; } = string.Empty;
    public NoteType Type { get; set; } = NoteType.General;
    public string? AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublic { get; set; } = true;
}

public enum NoteType
{
    General,
    Decision,
    ActionItem,
    Issue,
    Observation,
    Improvement
}

public class MeetingActionItem : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActionItemStatus Status { get; set; } = ActionItemStatus.Open;
    public ActionItemPriority Priority { get; set; } = ActionItemPriority.Medium;
    
    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }
    
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Task relationship
    public int? RelatedTaskId { get; set; }
    public TaskItem? RelatedTask { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = null!;
}

public enum ActionItemStatus
{
    Open,
    InProgress,
    Completed,
    Cancelled,
    Overdue
}

public enum ActionItemPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class StandupNote : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime Date { get; set; }
    public string? YesterdayAccomplishments { get; set; }
    public string? TodayGoals { get; set; }
    public string? Blockers { get; set; }
    public string? Notes { get; set; }
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsToday => Date.Date == DateTime.UtcNow.Date;
    
    // Meeting relationship (if this was part of a formal standup meeting)
    public int? MeetingId { get; set; }
    public Meeting? Meeting { get; set; }
}

public class RetrospectiveNote : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string Content { get; set; } = string.Empty;
    public RetrospectiveCategory Category { get; set; }
    
    public string? AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }
    
    public int Votes { get; set; } = 0;
    public string VoterIds { get; set; } = "[]";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsAnonymous { get; set; } = false;
    
    // Discussion
    public bool IsDiscussed { get; set; } = false;
    public DateTime? DiscussedAt { get; set; }
    public string? DiscussionSummary { get; set; }
    
    // Action items
    public List<MeetingActionItem> ActionItems { get; set; } = new List<MeetingActionItem>();
}

public enum RetrospectiveCategory
{
    WhatWentWell,    // Green/Positive
    WhatDidntGoWell, // Red/Issues
    Improvements,    // Yellow/Suggestions
    ActionItems,      // Blue/Tasks
    Appreciations     // Purple/Thanks
}
