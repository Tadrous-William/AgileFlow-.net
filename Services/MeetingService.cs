using System.Text.Json;
using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgileTaskManager.Services;

public class MeetingService : IMeetingService
{
    private readonly ApplicationDbContext _db;

    public MeetingService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<MeetingViewModel> CreateMeetingAsync(CreateMeetingViewModel model, string createdBy)
    {
        // Validate project exists
        var project = await _db.Projects.FindAsync(model.ProjectId);
        if (project == null)
            throw new ArgumentException("Project not found", nameof(model.ProjectId));

        // Validate sprint if specified
        Sprint? sprint = null;
        if (model.SprintId.HasValue)
        {
            sprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == model.SprintId.Value && s.ProjectId == model.ProjectId);
            if (sprint == null)
                throw new ArgumentException("Sprint not found or doesn't belong to this project", nameof(model.SprintId));
        }

        var meeting = new Meeting
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            Type = model.Type,
            Status = MeetingStatus.Scheduled,
            ScheduledAt = model.ScheduledAt,
            ProjectId = model.ProjectId,
            SprintId = sprint?.Id,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add participants
        foreach (var participantId in model.ParticipantIds.Distinct())
        {
            var participant = new MeetingParticipant
            {
                UserId = participantId,
                Status = ParticipantStatus.Invited,
                JoinedAt = model.ScheduledAt,
                IsFacilitator = participantId == model.FacilitatedBy
            };
            meeting.Participants.Add(participant);
        }

        // Set facilitator if specified
        if (!string.IsNullOrEmpty(model.FacilitatedBy))
        {
            meeting.FacilitatedBy = model.FacilitatedBy;
        }

        _db.Meetings.Add(meeting);
        await _db.SaveChangesAsync();

        return await MapToViewModelAsync(meeting);
    }

    public async Task<MeetingViewModel> UpdateMeetingAsync(UpdateMeetingViewModel model, string updatedBy)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == model.Id);

        if (meeting == null)
            throw new ArgumentException("Meeting not found", nameof(model.Id));

        // Check if meeting can be updated (not in progress or completed)
        if (meeting.Status == MeetingStatus.InProgress || meeting.Status == MeetingStatus.Completed)
            throw new InvalidOperationException("Cannot update a meeting that is in progress or completed");

        meeting.Title = model.Title.Trim();
        meeting.Description = model.Description?.Trim();
        meeting.ScheduledAt = model.ScheduledAt;
        meeting.FacilitatedBy = model.FacilitatedBy;
        meeting.UpdatedAt = DateTime.UtcNow;

        // Update participants
        var currentParticipantIds = meeting.Participants.Select(p => p.UserId).ToHashSet();
        var newParticipantIds = model.ParticipantIds.Distinct().ToHashSet();

        // Remove participants not in the new list
        var participantsToRemove = meeting.Participants.Where(p => !newParticipantIds.Contains(p.UserId)).ToList();
        foreach (var participant in participantsToRemove)
        {
            meeting.Participants.Remove(participant);
        }

        // Add new participants
        foreach (var participantId in newParticipantIds.Where(id => !currentParticipantIds.Contains(id)))
        {
            var participant = new MeetingParticipant
            {
                UserId = participantId,
                Status = ParticipantStatus.Invited,
                JoinedAt = model.ScheduledAt,
                IsFacilitator = participantId == model.FacilitatedBy
            };
            meeting.Participants.Add(participant);
        }

        await _db.SaveChangesAsync();

        return await MapToViewModelAsync(meeting);
    }

    public async Task<bool> DeleteMeetingAsync(int meetingId, string userId)
    {
        var meeting = await _db.Meetings.FirstOrDefaultAsync(m => m.Id == meetingId && m.CreatedBy == userId);
        if (meeting == null)
            return false;

        // Check if meeting can be deleted (not in progress or completed)
        if (meeting.Status == MeetingStatus.InProgress || meeting.Status == MeetingStatus.Completed)
            return false;

        _db.Meetings.Remove(meeting);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<MeetingViewModel> GetMeetingAsync(int meetingId)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Project)
            .Include(m => m.Sprint)
            .Include(m => m.CreatedByUser)
            .Include(m => m.Facilitator)
            .Include(m => m.Participants)
                .ThenInclude(p => p.User)
            .Include(m => m.Notes)
                .ThenInclude(n => n.Author)
            .Include(m => m.ActionItems)
                .ThenInclude(ai => ai.AssignedTo)
            .FirstOrDefaultAsync(m => m.Id == meetingId);

        if (meeting == null)
            throw new ArgumentException("Meeting not found", nameof(meetingId));

        return await MapToViewModelAsync(meeting);
    }

    public async Task<List<MeetingViewModel>> GetMeetingsAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.Meetings
            .Include(m => m.Project)
            .Include(m => m.Sprint)
            .Include(m => m.CreatedByUser)
            .Include(m => m.Facilitator)
            .Include(m => m.Participants)
                .ThenInclude(p => p.User)
            .Include(m => m.ActionItems)
                .ThenInclude(ai => ai.AssignedTo)
            .Where(m => m.ProjectId == projectId);

        if (startDate.HasValue)
            query = query.Where(m => m.ScheduledAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.ScheduledAt <= endDate.Value);

        var meetings = await query
            .OrderBy(m => m.ScheduledAt)
            .ToListAsync();

        var viewModels = new List<MeetingViewModel>();
        foreach (var meeting in meetings)
        {
            viewModels.Add(await MapToViewModelAsync(meeting));
        }

        return viewModels;
    }

    public async Task<MeetingViewModel> StartMeetingAsync(int meetingId, string startedBy)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == meetingId);

        if (meeting == null)
            throw new ArgumentException("Meeting not found", nameof(meetingId));

        if (meeting.Status != MeetingStatus.Scheduled)
            throw new InvalidOperationException("Meeting cannot be started");

        if (DateTime.UtcNow < meeting.ScheduledAt.AddMinutes(-5))
            throw new InvalidOperationException("Meeting cannot be started more than 5 minutes early");

        meeting.Status = MeetingStatus.InProgress;
        meeting.StartedAt = DateTime.UtcNow;
        meeting.UpdatedAt = DateTime.UtcNow;

        // Mark facilitator as joined
        var facilitatorParticipant = meeting.Participants.FirstOrDefault(p => p.UserId == meeting.FacilitatedBy);
        if (facilitatorParticipant != null)
        {
            facilitatorParticipant.Status = ParticipantStatus.Accepted;
            facilitatorParticipant.JoinedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return await MapToViewModelAsync(meeting);
    }

    public async Task<MeetingViewModel> EndMeetingAsync(int meetingId, string endedBy)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == meetingId);

        if (meeting == null)
            throw new ArgumentException("Meeting not found", nameof(meetingId));

        if (meeting.Status != MeetingStatus.InProgress)
            throw new InvalidOperationException("Meeting is not in progress");

        meeting.Status = MeetingStatus.Completed;
        meeting.EndedAt = DateTime.UtcNow;
        meeting.UpdatedAt = DateTime.UtcNow;

        // Mark all participants as left
        foreach (var participant in meeting.Participants.Where(p => !p.LeftAt.HasValue))
        {
            participant.LeftAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return await MapToViewModelAsync(meeting);
    }

    public async Task<bool> JoinMeetingAsync(int meetingId, string userId)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == meetingId);

        if (meeting == null)
            return false;

        if (meeting.Status != MeetingStatus.InProgress)
            return false;

        var participant = meeting.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            return false;

        participant.Status = ParticipantStatus.Accepted;
        participant.JoinedAt = DateTime.UtcNow;
        participant.LeftAt = null;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LeaveMeetingAsync(int meetingId, string userId)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == meetingId);

        if (meeting == null)
            return false;

        var participant = meeting.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            return false;

        participant.LeftAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<StandupNoteViewModel> CreateStandupNoteAsync(CreateStandupNoteViewModel model, string userId)
    {
        // Check if user already has a standup for today
        var today = DateTime.UtcNow.Date;
        var existingStandup = await _db.StandupNotes
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Date.Date == today && s.ProjectId == model.ProjectId);

        if (existingStandup != null)
        {
            // Update existing standup
            existingStandup.YesterdayAccomplishments = model.YesterdayAccomplishments;
            existingStandup.TodayGoals = model.TodayGoals;
            existingStandup.Blockers = model.Blockers;
            existingStandup.Notes = model.Notes;
            existingStandup.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await MapStandupToViewModelAsync(existingStandup);
        }

        var standup = new StandupNote
        {
            UserId = userId,
            Date = today,
            YesterdayAccomplishments = model.YesterdayAccomplishments,
            TodayGoals = model.TodayGoals,
            Blockers = model.Blockers,
            Notes = model.Notes,
            ProjectId = model.ProjectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.StandupNotes.Add(standup);
        await _db.SaveChangesAsync();

        return await MapStandupToViewModelAsync(standup);
    }

    public async Task<List<StandupNoteViewModel>> GetStandupNotesAsync(int projectId, DateTime? date = null)
    {
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;

        var standups = await _db.StandupNotes
            .Include(s => s.User)
            .Include(s => s.Project)
            .Where(s => s.ProjectId == projectId && s.Date.Date == targetDate)
            .OrderBy(s => s.User.FullName)
            .ToListAsync();

        var viewModels = new List<StandupNoteViewModel>();
        foreach (var standup in standups)
        {
            viewModels.Add(await MapStandupToViewModelAsync(standup));
        }

        return viewModels;
    }

    public async Task<RetrospectiveNoteViewModel> CreateRetrospectiveNoteAsync(CreateRetrospectiveNoteViewModel model, string userId)
    {
        var note = new RetrospectiveNote
        {
            MeetingId = model.MeetingId,
            Content = model.Content.Trim(),
            Category = model.Category,
            AuthorId = model.IsAnonymous ? null : userId,
            IsAnonymous = model.IsAnonymous,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.RetrospectiveNotes.Add(note);
        await _db.SaveChangesAsync();

        return await MapRetrospectiveToViewModelAsync(note, userId);
    }

    public async Task<List<RetrospectiveNoteViewModel>> GetRetrospectiveNotesAsync(int meetingId)
    {
        var notes = await _db.RetrospectiveNotes
            .Include(rn => rn.Author)
            .Include(rn => rn.ActionItems)
                .ThenInclude(ai => ai.AssignedTo)
            .Where(rn => rn.MeetingId == meetingId)
            .OrderBy(rn => rn.Category)
                .ThenByDescending(rn => rn.Votes)
                .ThenBy(rn => rn.CreatedAt)
            .ToListAsync();

        var viewModels = new List<RetrospectiveNoteViewModel>();
        foreach (var note in notes)
        {
            viewModels.Add(await MapRetrospectiveToViewModelAsync(note, null));
        }

        return viewModels;
    }

    public async Task<MeetingActionItemViewModel> CreateActionItemAsync(int meetingId, CreateActionItemViewModel model, string createdBy)
    {
        var actionItem = new MeetingActionItem
        {
            MeetingId = meetingId,
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            Priority = model.Priority,
            AssignedToId = model.AssignedToId,
            DueDate = model.DueDate,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _db.MeetingActionItems.Add(actionItem);
        await _db.SaveChangesAsync();

        return await MapActionItemToViewModelAsync(actionItem);
    }

    public async Task<MeetingActionItemViewModel> UpdateActionItemAsync(UpdateActionItemViewModel model, string updatedBy)
    {
        var actionItem = await _db.MeetingActionItems.FindAsync(model.Id);
        if (actionItem == null)
            throw new ArgumentException("Action item not found", nameof(model.Id));

        actionItem.Title = model.Title.Trim();
        actionItem.Description = model.Description?.Trim();
        actionItem.Status = model.Status;
        actionItem.Priority = model.Priority;
        actionItem.AssignedToId = model.AssignedToId;
        actionItem.DueDate = model.DueDate;

        if (model.Status == ActionItemStatus.Completed && !actionItem.CompletedAt.HasValue)
        {
            actionItem.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return await MapActionItemToViewModelAsync(actionItem);
    }

    public async Task<bool> DeleteActionItemAsync(int actionItemId, string userId)
    {
        var actionItem = await _db.MeetingActionItems.FirstOrDefaultAsync(ai => ai.Id == actionItemId && ai.CreatedBy == userId);
        if (actionItem == null)
            return false;

        _db.MeetingActionItems.Remove(actionItem);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<MeetingDashboardViewModel> GetDashboardAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        // Get user's projects
        var userProjects = await _db.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync();

        // Today's meetings
        var todayMeetings = await _db.Meetings
            .Include(m => m.Sprint)
            .Include(m => m.CreatedByUser)
            .Include(m => m.Facilitator)
            .Include(m => m.Participants)
                .ThenInclude(p => p.User)
            .Include(m => m.Project)
            .Include(m => m.ActionItems)
                .ThenInclude(ai => ai.AssignedTo)
            .Where(m => userProjects.Contains(m.ProjectId) && 
                       m.ScheduledAt.Date == today &&
                       m.Status != MeetingStatus.Cancelled)
            .OrderBy(m => m.ScheduledAt)
            .ToListAsync();

        // Upcoming meetings (next 7 days)
        var upcomingMeetings = await _db.Meetings
            .Include(m => m.Sprint)
            .Include(m => m.CreatedByUser)
            .Include(m => m.Facilitator)
            .Include(m => m.Participants)
                .ThenInclude(p => p.User)
            .Include(m => m.Project)
            .Include(m => m.ActionItems)
                .ThenInclude(ai => ai.AssignedTo)
            .Where(m => userProjects.Contains(m.ProjectId) && 
                       m.ScheduledAt > now &&
                       m.ScheduledAt <= today.AddDays(7) &&
                       m.Status == MeetingStatus.Scheduled)
            .OrderBy(m => m.ScheduledAt)
            .ToListAsync();

        // Recent standups (last 3 days)
        var recentStandups = await _db.StandupNotes
            .Include(s => s.User)
            .Include(s => s.Project)
            .Where(s => userProjects.Contains(s.ProjectId) && s.Date >= today.AddDays(-2))
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.User.FullName)
            .Take(20)
            .ToListAsync();

        // Pending action items
        var pendingActionItems = await _db.MeetingActionItems
            .Include(ai => ai.AssignedTo)
            .Include(ai => ai.Meeting)
                .ThenInclude(m => m.Project)
            .Where(ai => ai.AssignedToId == userId && 
                        ai.Status == ActionItemStatus.Open)
            .OrderBy(ai => ai.DueDate ?? DateTime.MaxValue)
            .ToListAsync();

        // Overdue action items
        var overdueActionItems = pendingActionItems
            .Where(ai => ai.DueDate.HasValue && ai.DueDate.Value < today)
            .ToList();

        var dashboard = new MeetingDashboardViewModel
        {
            Today = now,
            TodayMeetingCount = todayMeetings.Count,
            UpcomingMeetingCount = upcomingMeetings.Count,
            PendingActionItemCount = pendingActionItems.Count,
            OverdueActionItemCount = overdueActionItems.Count,
            HasStandupToday = recentStandups.Any(s => s.IsToday)
        };

        // Map view models
        foreach (var meeting in todayMeetings)
        {
            dashboard.TodayMeetings.Add(await MapToViewModelAsync(meeting));
        }

        foreach (var meeting in upcomingMeetings)
        {
            dashboard.UpcomingMeetings.Add(await MapToViewModelAsync(meeting));
        }

        foreach (var standup in recentStandups)
        {
            dashboard.RecentStandups.Add(await MapStandupToViewModelAsync(standup));
        }

        foreach (var actionItem in pendingActionItems)
        {
            dashboard.PendingActionItems.Add(await MapActionItemToViewModelAsync(actionItem));
        }

        foreach (var actionItem in overdueActionItems)
        {
            dashboard.OverdueActionItems.Add(await MapActionItemToViewModelAsync(actionItem));
        }

        return dashboard;
    }

    public async Task<MeetingAnalyticsViewModel> GetAnalyticsAsync(int projectId, DateTime startDate, DateTime endDate)
    {
        var meetings = await _db.Meetings
            .Include(m => m.Participants)
                .ThenInclude(p => p.User)
            .Include(m => m.ActionItems)
            .Where(m => m.ProjectId == projectId && 
                       m.ScheduledAt.Date >= startDate.Date && 
                       m.ScheduledAt.Date <= endDate.Date)
            .ToListAsync();

        var totalMeetings = meetings.Count;
        var completedMeetings = meetings.Count(m => m.Status == MeetingStatus.Completed);
        var cancelledMeetings = meetings.Count(m => m.Status == MeetingStatus.Cancelled);
        
        var totalDuration = meetings
            .Where(m => m.StartedAt.HasValue && m.EndedAt.HasValue)
            .Aggregate(TimeSpan.Zero, (sum, m) => sum + (m.EndedAt!.Value - m.StartedAt!.Value));
        
        var averageDuration = completedMeetings > 0 
            ? TimeSpan.FromTicks(totalDuration.Ticks / completedMeetings) 
            : TimeSpan.Zero;

        var totalParticipants = meetings.SelectMany(m => m.Participants).Count();
        var totalActionItems = meetings.SelectMany(m => m.ActionItems).Count();
        var completedActionItems = meetings.SelectMany(m => m.ActionItems).Count(ai => ai.Status == ActionItemStatus.Completed);

        // Meetings by type
        var meetingsByType = meetings
            .GroupBy(m => m.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        // Meetings by status
        var meetingsByStatus = meetings
            .GroupBy(m => m.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        // Participant analytics
        var topParticipants = meetings
            .SelectMany(m => m.Participants)
            .GroupBy(p => p.UserId)
            .Select(g => new ParticipantAnalyticsViewModel
            {
                UserId = g.Key,
                UserName = g.First().User?.FullName ?? "Unknown",
                MeetingsAttended = g.Count(p => p.Status == ParticipantStatus.Accepted),
                TotalAttendanceTime = g.Aggregate(TimeSpan.Zero, (sum, p) => 
                    sum + (p.LeftAt.HasValue ? p.LeftAt.Value - p.JoinedAt : TimeSpan.Zero)),
                ActionItemsAssigned = meetings.SelectMany(m => m.ActionItems).Count(ai => ai.AssignedToId == g.Key),
                ActionItemsCompleted = meetings.SelectMany(m => m.ActionItems)
                    .Count(ai => ai.AssignedToId == g.Key && ai.Status == ActionItemStatus.Completed)
            })
            .OrderByDescending(p => p.MeetingsAttended)
            .Take(10)
            .ToList();

        // Action item stats
        var allActionItems = meetings.SelectMany(m => m.ActionItems).ToList();
        var actionItemStats = Enum.GetValues<ActionItemStatus>()
            .Select(status => new ActionItemAnalyticsViewModel
            {
                Status = status,
                Count = allActionItems.Count(ai => ai.Status == status),
                Percentage = allActionItems.Count > 0 
                    ? (double)allActionItems.Count(ai => ai.Status == status) / allActionItems.Count * 100 
                    : 0
            })
            .ToList();

        return new MeetingAnalyticsViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalMeetings = totalMeetings,
            CompletedMeetings = completedMeetings,
            CancelledMeetings = cancelledMeetings,
            CompletionRate = totalMeetings > 0 ? (double)completedMeetings / totalMeetings * 100 : 0,
            AverageMeetingDuration = averageDuration,
            TotalParticipants = totalParticipants,
            TotalActionItems = totalActionItems,
            CompletedActionItems = completedActionItems,
            MeetingsByType = meetingsByType,
            MeetingsByStatus = meetingsByStatus,
            TopParticipants = topParticipants,
            ActionItemStats = actionItemStats
        };
    }

    private Task<MeetingViewModel> MapToViewModelAsync(Meeting meeting)
    {
        return Task.FromResult(new MeetingViewModel
        {
            Id = meeting.Id,
            Title = meeting.Title,
            Description = meeting.Description,
            Type = meeting.Type,
            Status = meeting.Status,
            ScheduledAt = meeting.ScheduledAt,
            StartedAt = meeting.StartedAt,
            EndedAt = meeting.EndedAt,
            Duration = meeting.Duration,
            ProjectId = meeting.ProjectId,
            ProjectName = meeting.Project?.Name ?? "",
            SprintId = meeting.SprintId,
            SprintName = meeting.Sprint?.Name ?? "",
            CreatedBy = meeting.CreatedBy,
            CreatedByName = meeting.CreatedByUser?.FullName ?? "",
            FacilitatedBy = meeting.FacilitatedBy,
            FacilitatorName = meeting.Facilitator?.FullName ?? "",
            CreatedAt = meeting.CreatedAt,
            UpdatedAt = meeting.UpdatedAt,
            Participants = meeting.Participants.Select(p => new MeetingParticipantViewModel
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User?.FullName ?? "",
                Status = p.Status,
                RespondedAt = p.RespondedAt,
                ResponseNote = p.ResponseNote,
                IsFacilitator = p.IsFacilitator,
                JoinedAt = p.JoinedAt,
                LeftAt = p.LeftAt
            }).ToList(),
            Notes = meeting.Notes.Select(n => new MeetingNoteViewModel
            {
                Id = n.Id,
                MeetingId = n.MeetingId,
                Content = n.Content,
                Type = n.Type,
                AuthorId = n.AuthorId,
                AuthorName = n.Author?.FullName ?? "",
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                IsPublic = n.IsPublic
            }).ToList(),
            ActionItems = meeting.ActionItems.Select(ai => new MeetingActionItemViewModel
            {
                Id = ai.Id,
                MeetingId = ai.MeetingId,
                Title = ai.Title,
                Description = ai.Description,
                Status = ai.Status,
                Priority = ai.Priority,
                AssignedToId = ai.AssignedToId,
                AssignedToName = ai.AssignedTo?.FullName ?? "",
                DueDate = ai.DueDate,
                CreatedAt = ai.CreatedAt,
                CompletedAt = ai.CompletedAt,
                RelatedTaskId = ai.RelatedTaskId,
                CreatedBy = ai.CreatedBy,
                CreatedByName = ai.CreatedByUser?.FullName ?? ""
            }).ToList()
        });
    }

    private async Task<StandupNoteViewModel> MapStandupToViewModelAsync(StandupNote standup)
    {
        return new StandupNoteViewModel
        {
            Id = standup.Id,
            UserId = standup.UserId,
            UserName = standup.User?.FullName ?? "",
            Date = standup.Date,
            YesterdayAccomplishments = standup.YesterdayAccomplishments,
            TodayGoals = standup.TodayGoals,
            Blockers = standup.Blockers,
            Notes = standup.Notes,
            ProjectId = standup.ProjectId,
            ProjectName = standup.Project?.Name ?? "",
            CreatedAt = standup.CreatedAt,
            UpdatedAt = standup.UpdatedAt,
            MeetingId = standup.MeetingId,
            IsToday = standup.Date.Date == DateTime.UtcNow.Date
        };
    }

    private async Task<RetrospectiveNoteViewModel> MapRetrospectiveToViewModelAsync(RetrospectiveNote note, string? currentUserId)
    {
        return new RetrospectiveNoteViewModel
        {
            Id = note.Id,
            MeetingId = note.MeetingId,
            Content = note.Content,
            Category = note.Category,
            AuthorId = note.AuthorId,
            AuthorName = note.IsAnonymous ? "Anonymous" : (note.Author?.FullName ?? ""),
            IsAnonymous = note.IsAnonymous,
            Votes = note.Votes,
            HasVoted = currentUserId != null && SafeDeserializeVoterIds(note.VoterIds).Contains(currentUserId),
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            IsDiscussed = note.IsDiscussed,
            DiscussedAt = note.DiscussedAt,
            DiscussionSummary = note.DiscussionSummary,
            ActionItems = note.ActionItems.Select(ai => new MeetingActionItemViewModel
            {
                Id = ai.Id,
                MeetingId = ai.MeetingId,
                Title = ai.Title,
                Description = ai.Description,
                Status = ai.Status,
                Priority = ai.Priority,
                AssignedToId = ai.AssignedToId,
                AssignedToName = ai.AssignedTo?.FullName ?? "",
                DueDate = ai.DueDate,
                CreatedAt = ai.CreatedAt,
                CompletedAt = ai.CompletedAt,
                CreatedBy = ai.CreatedBy,
                CreatedByName = ai.CreatedByUser?.FullName ?? ""
            }).ToList()
        };
    }

    private static List<string> SafeDeserializeVoterIds(string? voterIds)
    {
        if (string.IsNullOrWhiteSpace(voterIds)) return new List<string>();
        try { return JsonSerializer.Deserialize<List<string>>(voterIds) ?? new List<string>(); }
        catch (JsonException) { return new List<string>(); }
    }

    private async Task<MeetingActionItemViewModel> MapActionItemToViewModelAsync(MeetingActionItem actionItem)
    {
        return new MeetingActionItemViewModel
        {
            Id = actionItem.Id,
            MeetingId = actionItem.MeetingId,
            Title = actionItem.Title,
            Description = actionItem.Description,
            Status = actionItem.Status,
            Priority = actionItem.Priority,
            AssignedToId = actionItem.AssignedToId,
            AssignedToName = actionItem.AssignedTo?.FullName ?? "",
            DueDate = actionItem.DueDate,
            CreatedAt = actionItem.CreatedAt,
            CompletedAt = actionItem.CompletedAt,
            RelatedTaskId = actionItem.RelatedTaskId,
            RelatedTaskTitle = actionItem.RelatedTask?.Title,
            CreatedBy = actionItem.CreatedBy,
            CreatedByName = actionItem.CreatedByUser?.FullName ?? ""
        };
    }
}
