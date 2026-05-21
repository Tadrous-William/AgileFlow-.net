using AgileTaskManager.Models.Enums;
using TaskStatus = AgileTaskManager.Models.Enums.TaskStatus;

namespace AgileTaskManager.Models.ViewModels;

public class KanbanBoardViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int? SprintId { get; set; }
    public string? SprintName { get; set; }
    public KanbanColumnViewModel[] Columns { get; set; } = Array.Empty<KanbanColumnViewModel>();
}

public class KanbanColumnViewModel
{
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public KanbanTaskViewModel[] Tasks { get; set; } = Array.Empty<KanbanTaskViewModel>();
}

public class KanbanTaskViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Send enums as strings for JS friendliness
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsOverdue { get; set; }
    public int? DependsOnId { get; set; }
    public string? DependsOnTitle { get; set; }
    public string? ProjectName { get; set; }
    public string? SprintName { get; set; }
}

public class MoveTaskViewModel
{
    public int TaskId { get; set; }
    public TaskStatus NewStatus { get; set; }
}

public class KanbanBoardRequestViewModel
{
    public int ProjectId { get; set; }
    public int? SprintId { get; set; }
}
