using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AgileTaskManager.Hubs;

[Authorize]
public class TaskHub : Hub
{
    // Client joins a task-specific group to receive live updates for that task
    public async Task JoinTaskGroup(string taskId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"task-{taskId}");

    public async Task LeaveTaskGroup(string taskId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task-{taskId}");

    // Join Kanban board group for real-time updates
    public async Task JoinKanbanBoard(int projectId, int? sprintId = null)
    {
        var groupName = sprintId.HasValue 
            ? $"kanban-project-{projectId}-sprint-{sprintId.Value}"
            : $"kanban-project-{projectId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveKanbanBoard(int projectId, int? sprintId = null)
    {
        var groupName = sprintId.HasValue 
            ? $"kanban-project-{projectId}-sprint-{sprintId.Value}"
            : $"kanban-project-{projectId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    // Broadcast new comment to all group members
    public async Task SendComment(string taskId, string authorName, string content, string createdAt)
        => await Clients.OthersInGroup($"task-{taskId}")
            .SendAsync("ReceiveComment", authorName, content, createdAt);

    // Broadcast status change
    public async Task SendStatusUpdate(string taskId, string newStatus, string changedBy)
        => await Clients.Group($"task-{taskId}")
            .SendAsync("ReceiveStatusUpdate", newStatus, changedBy);

    // Broadcast Kanban board task move
    public async Task SendTaskMove(int projectId, int? sprintId, int taskId, string oldStatus, string newStatus, string movedBy)
    {
        var groupName = sprintId.HasValue 
            ? $"kanban-project-{projectId}-sprint-{sprintId.Value}"
            : $"kanban-project-{projectId}";
        
        await Clients.Group(groupName)
            .SendAsync("ReceiveTaskMove", new { taskId, oldStatus, newStatus, movedBy });
    }

    // Broadcast task creation/update on Kanban board
    public async Task SendKanbanTaskUpdate(int projectId, int? sprintId, object taskData, string action)
    {
        var groupName = sprintId.HasValue 
            ? $"kanban-project-{projectId}-sprint-{sprintId.Value}"
            : $"kanban-project-{projectId}";
        
        await Clients.Group(groupName)
            .SendAsync("ReceiveKanbanTaskUpdate", new { taskData, action });
    }

    // Broadcast notification to specific user
    public async Task SendNotification(string userId, string message)
        => await Clients.User(userId)
            .SendAsync("ReceiveNotification", message);

    public override async Task OnConnectedAsync()
    {
        // Add user to their personal notification group
        var userId = Context.UserIdentifier;
        if (userId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }
}
