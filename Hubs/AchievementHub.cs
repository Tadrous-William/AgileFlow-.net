using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using AgileTaskManager.Models.ViewModels;

namespace AgileTaskManager.Hubs;

[Authorize]
public class AchievementHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        if (Context.UserIdentifier != userId && Context.User?.IsInRole("Admin") != true)
            throw new HubException("Unauthorized");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task JoinProjectGroup(int projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
    }

    public async Task LeaveProjectGroup(int projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
    }
}
