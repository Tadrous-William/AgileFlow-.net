using AgileTaskManager.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace AgileTaskManager.Models.ViewModels;

public class ProjectListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TeamSize { get; set; }
    public int SprintsCount { get; set; }
}

public class AddProjectMemberViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Developer;
}

public class SprintCreateViewModel
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SprintUpdateStatusViewModel
{
    public SprintStatus Status { get; set; }
}

public class SprintViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public SprintStatus Status { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public decimal Progress { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
}

public class CreateSprintViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    [Required]
    public int ProjectId { get; set; }
}

public class UpdateSprintViewModel
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; }
}

public class SprintBurndownViewModel
{
    public DateTime Date { get; set; }
    public int IdealRemaining { get; set; }
    public int ActualRemaining { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
}
