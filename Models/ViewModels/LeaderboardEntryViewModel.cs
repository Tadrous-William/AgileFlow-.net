namespace AgileTaskManager.Models.ViewModels;

public class LeaderboardEntryViewModel
{
    public int Rank { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalXP { get; set; }
    public int Level { get; set; }
    public int BadgeCount { get; set; }
    public int Streak { get; set; }
    public int TasksCompleted { get; set; }
    public string LevelTitle { get; set; } = string.Empty;
    public string LevelColor { get; set; } = string.Empty;
}
