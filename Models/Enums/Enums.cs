namespace AgileTaskManager.Models.Enums;

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public enum TaskStatus
{
    ToDo,
    InProgress,
    Testing,
    Done
}

public enum SprintStatus
{
    Planned,
    Active,
    Completed
}

public enum ProjectMemberRole
{
    Admin,
    TeamLead,
    Developer,
    Viewer
}

public enum UserRole
{
    Admin,
    TeamLead,
    Developer,
    Viewer,
    Member,
    Client
}
