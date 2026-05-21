using AgileTaskManager.Models.Entities;

namespace AgileTaskManager.Models.ViewModels;

public class MeetingViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MeetingType Type { get; set; }
    public MeetingStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    public string DurationFormatted => Duration.HasValue ? MeetingFormattingHelper.FormatDuration(Duration.Value) : "Not started";
    
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    
    public int? SprintId { get; set; }
    public string SprintName { get; set; } = string.Empty;
    
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    
    public string? FacilitatedBy { get; set; }
    public string? FacilitatorName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public List<MeetingParticipantViewModel> Participants { get; set; } = new();
    public List<MeetingNoteViewModel> Notes { get; set; } = new();
    public List<MeetingActionItemViewModel> ActionItems { get; set; } = new();
    
    public int ParticipantCount => Participants.Count;
    public int AcceptedCount => Participants.Count(p => p.Status == ParticipantStatus.Accepted);
    public int ActionItemsCount => ActionItems.Count;
    public int OpenActionItemsCount => ActionItems.Count(ai => ai.Status == ActionItemStatus.Open);
    
    public bool IsScheduled => Status == MeetingStatus.Scheduled;
    public bool IsInProgress => Status == MeetingStatus.InProgress;
    public bool IsCompleted => Status == MeetingStatus.Completed;
    public bool CanStart => IsScheduled && DateTime.UtcNow >= ScheduledAt;
    public bool IsPast => ScheduledAt < DateTime.UtcNow && !IsInProgress;
}

public class CreateMeetingViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MeetingType Type { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int ProjectId { get; set; }
    public int? SprintId { get; set; }
    public List<string> ParticipantIds { get; set; } = new();
    public string? FacilitatedBy { get; set; }
}

public class UpdateMeetingViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public List<string> ParticipantIds { get; set; } = new();
    public string? FacilitatedBy { get; set; }
}

public class MeetingParticipantViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseNote { get; set; }
    public bool IsFacilitator { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public TimeSpan? AttendanceDuration => LeftAt.HasValue ? LeftAt.Value - JoinedAt : null;
}

public class MeetingNoteViewModel
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public string Content { get; set; } = string.Empty;
    public NoteType Type { get; set; }
    public string? AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublic { get; set; }
}

public class MeetingActionItemViewModel
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActionItemStatus Status { get; set; }
    public ActionItemPriority Priority { get; set; }
    public string? AssignedToId { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? RelatedTaskId { get; set; }
    public string? RelatedTaskTitle { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != ActionItemStatus.Completed;
    public bool IsCompleted => Status == ActionItemStatus.Completed;
    public bool IsOpen => Status == ActionItemStatus.Open;
}

public class StandupNoteViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? YesterdayAccomplishments { get; set; }
    public string? TodayGoals { get; set; }
    public string? Blockers { get; set; }
    public string? Notes { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? MeetingId { get; set; }
    
    public bool HasBlockers => !string.IsNullOrWhiteSpace(Blockers);
    public bool HasAccomplishments => !string.IsNullOrWhiteSpace(YesterdayAccomplishments);
    public bool HasGoals => !string.IsNullOrWhiteSpace(TodayGoals);
    public bool IsToday { get; set; }
}

public class CreateStandupNoteViewModel
{
    public string? YesterdayAccomplishments { get; set; }
    public string? TodayGoals { get; set; }
    public string? Blockers { get; set; }
    public string? Notes { get; set; }
    public int ProjectId { get; set; }
}

public class RetrospectiveNoteViewModel
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public string Content { get; set; } = string.Empty;
    public RetrospectiveCategory Category { get; set; }
    public string? AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public int Votes { get; set; }
    public bool HasVoted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDiscussed { get; set; }
    public DateTime? DiscussedAt { get; set; }
    public string? DiscussionSummary { get; set; }
    public List<MeetingActionItemViewModel> ActionItems { get; set; } = new();
    
    public string CategoryDisplayName => Category.ToString().Replace("What", "What ").Replace("Didnt", "Didn't");
    public string CategoryColor => Category switch
    {
        RetrospectiveCategory.WhatWentWell => "#28a745",      // Green
        RetrospectiveCategory.WhatDidntGoWell => "#dc3545", // Red
        RetrospectiveCategory.Improvements => "#ffc107",    // Yellow
        RetrospectiveCategory.ActionItems => "#007bff",     // Blue
        RetrospectiveCategory.Appreciations => "#6f42c1",   // Purple
        _ => "#6c757d"
    };
}

public class CreateRetrospectiveNoteViewModel
{
    public int MeetingId { get; set; }
    public string Content { get; set; } = string.Empty;
    public RetrospectiveCategory Category { get; set; }
    public bool IsAnonymous { get; set; } = false;
}

public class MeetingDashboardViewModel
{
    public DateTime Today { get; set; }
    public List<MeetingViewModel> UpcomingMeetings { get; set; } = new();
    public List<MeetingViewModel> TodayMeetings { get; set; } = new();
    public List<MeetingViewModel> PastMeetings { get; set; } = new();
    public List<StandupNoteViewModel> RecentStandups { get; set; } = new();
    public List<MeetingActionItemViewModel> PendingActionItems { get; set; } = new();
    public List<MeetingActionItemViewModel> OverdueActionItems { get; set; } = new();
    
    public int TodayMeetingCount { get; set; }
    public int UpcomingMeetingCount { get; set; }
    public int PendingActionItemCount { get; set; }
    public int OverdueActionItemCount { get; set; }
    public bool HasStandupToday { get; set; }
}

public class MeetingAnalyticsViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalMeetings { get; set; }
    public int CompletedMeetings { get; set; }
    public int CancelledMeetings { get; set; }
    public double CompletionRate { get; set; }
    public TimeSpan AverageMeetingDuration { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalActionItems { get; set; }
    public int CompletedActionItems { get; set; }
    
    // Breakdown by type
    public Dictionary<MeetingType, int> MeetingsByType { get; set; } = new();
    
    // Breakdown by status
    public Dictionary<MeetingStatus, int> MeetingsByStatus { get; set; } = new();
    
    // Participant analytics
    public List<ParticipantAnalyticsViewModel> TopParticipants { get; set; } = new();
    
    // Action item analytics
    public List<ActionItemAnalyticsViewModel> ActionItemStats { get; set; } = new();
}

public class ParticipantAnalyticsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int MeetingsAttended { get; set; }
    public TimeSpan TotalAttendanceTime { get; set; }
    public int ActionItemsAssigned { get; set; }
    public int ActionItemsCompleted { get; set; }
}

public class ActionItemAnalyticsViewModel
{
    public ActionItemStatus Status { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class QuickStartMeetingViewModel
{
    public MeetingType Type { get; set; }
    public int ProjectId { get; set; }
    public int? SprintId { get; set; }
    public List<string> ParticipantIds { get; set; } = new();
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    public string? Title { get; set; }
}

// Helper methods
public static class MeetingFormattingHelper
{
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{duration.Hours}h {duration.Minutes}m";
        
        return $"{duration.Minutes}m";
    }
    
    public static string GetMeetingTypeDisplayName(MeetingType type)
    {
        return type switch
        {
            MeetingType.DailyStandup => "Daily Standup",
            MeetingType.SprintPlanning => "Sprint Planning",
            MeetingType.SprintReview => "Sprint Review",
            MeetingType.SprintRetrospective => "Sprint Retrospective",
            MeetingType.TeamMeeting => "Team Meeting",
            MeetingType.OneOnOne => "1-on-1",
            MeetingType.ClientMeeting => "Client Meeting",
            MeetingType.Other => "Other",
            _ => type.ToString()
        };
    }
}

public class CreateActionItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActionItemPriority Priority { get; set; } = ActionItemPriority.Medium;
    public string? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateActionItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActionItemStatus Status { get; set; }
    public ActionItemPriority Priority { get; set; }
    public string? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
}
