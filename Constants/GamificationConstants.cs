namespace AgileTaskManager.Constants;

public static class GamificationConstants
{
    // XP Values
    public const int TASK_COMPLETION_XP = 50;
    public const int COMMENT_ADDED_XP = 10;
    public const int BADGE_BASE_XP = 25;
    
    // Streak Milestones
    public static readonly int[] STREAK_MILESTONES = { 3, 7, 14, 30, 60, 100 };
    
    // Level Definitions
    public const int LEVEL_1_XP = 0;
    public const int LEVEL_2_XP = 100;
    public const int LEVEL_3_XP = 250;
    public const int LEVEL_4_XP = 500;
    public const int LEVEL_5_XP = 1000;
    public const int LEVEL_6_XP = 2000;
    public const int LEVEL_7_XP = 5000;
    public const int LEVEL_8_XP = 10000;
    
    // Streak Rules
    public const int STREAK_RESET_HOURS = 48;
    
    // Leaderboard
    public const int DEFAULT_LEADERBOARD_COUNT = 10;
    public const int GLOBAL_LEADERBOARD_COUNT = 10;
    
    // Notification Limits
    public const int MAX_RECENT_BADGES = 5;
    public const int MAX_RECENT_ACTIVITY = 10;
    public const int MAX_RECENT_TASKS = 10;
    
    // Dashboard Limits
    public const int TOP_PERFORMERS_COUNT = 10;
    public const int RECENT_USERS_COUNT = 10;
    public const int UPCOMING_DEADLINES_COUNT = 5;
    public const int BLOCKED_TASKS_COUNT = 5;
    public const int UPCOMING_MEETINGS_COUNT = 3;
    public const string RECENT_TASKS = "recent_tasks";
}
