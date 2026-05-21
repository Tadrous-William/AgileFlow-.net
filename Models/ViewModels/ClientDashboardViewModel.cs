namespace AgileTaskManager.Models.ViewModels;

public class ClientDashboardViewModel
{
    // Client Overview
    public string ClientName { get; set; } = string.Empty;
    public string ClientCompany { get; set; } = string.Empty;
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public DateTime FirstProjectDate { get; set; }
    
    // Project Statistics
    public List<ClientProjectViewModel> Projects { get; set; } = new();
    public Dictionary<string, int> ProjectsByStatus { get; set; } = new();
    
    // Task Statistics (only tasks visible to client)
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    
    // Progress Overview
    public double OverallProgress { get; set; }
    public int OnTimeDeliveries { get; set; }
    public int DelayedDeliverables { get; set; }
    public double AverageCompletionTime { get; set; }
    
    // Recent Activity
    public List<TaskListViewModel> RecentlyCompletedTasks { get; set; } = new();
    public List<TaskListViewModel> TasksRequiringFeedback { get; set; } = new();
    public List<FeedbackViewModel> RecentFeedback { get; set; } = new();
    public List<ActivityLogViewModel> RecentActivity { get; set; } = new();
    
    // Feedback & Ratings
    public double AverageRating { get; set; }
    public int TotalFeedbackGiven { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    
    // Upcoming Milestones
    public List<ProjectMilestoneViewModel> UpcomingMilestones { get; set; } = new();
    public List<ProjectMilestoneViewModel> OverdueMilestones { get; set; } = new();
    
    // Communication
    public List<MeetingSummaryViewModel> UpcomingMeetings { get; set; } = new();
    public List<CommentViewModel> RecentComments { get; set; } = new();
    
    // Financial Overview (if applicable)
    public List<ProjectFinancialViewModel> ProjectFinancials { get; set; } = new();
    public decimal TotalProjectValue { get; set; }
    public decimal InvoicedAmount { get; set; }
    public decimal PendingAmount { get; set; }
    
    // Documents & Deliverables
    public List<DeliverableViewModel> RecentDeliverables { get; set; } = new();
    public List<DeliverableViewModel> PendingDeliverables { get; set; } = new();
    
    public DateTime LastDataUpdate { get; set; } = DateTime.UtcNow;
}

public class ClientProjectViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
    public double ProgressPercentage { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public string ProjectManager { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
    public decimal? Spent { get; set; }
    public bool IsOnTrack { get; set; }
    public List<string> Risks { get; set; } = new();
}

public class ProjectMilestoneViewModel
{
    public int MilestoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = string.Empty; // Upcoming, Overdue, Completed
    public int AssociatedTasks { get; set; }
    public int CompletedTasks { get; set; }
    public bool IsCritical { get; set; }
}

public class ProjectFinancialViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalBudget { get; set; }
    public decimal SpentToDate { get; set; }
    public decimal InvoicedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public DateTime? LastInvoiceDate { get; set; }
    public DateTime? NextInvoiceDate { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class DeliverableViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Document, Software, Report, etc.
    public DateTime DueDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Delivered, Approved
    public string DeliveryMethod { get; set; } = string.Empty;
    public string? DownloadUrl { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
}
