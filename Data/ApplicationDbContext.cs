using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.Interfaces;
using AgileTaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgileTaskManager.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added && e.Entity.TenantId == 0))
        {
            entry.Entity.TenantId = _tenantContext.TenantId;
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    public new DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<MeetingNote> MeetingNotes { get; set; }
    public DbSet<StandupNote> StandupNotes { get; set; }
    public DbSet<RetrospectiveNote> RetrospectiveNotes { get; set; }
    public DbSet<MeetingActionItem> MeetingActionItems { get; set; }
    public DbSet<TaskDependency> TaskDependencies { get; set; }
    public DbSet<Sprint> Sprints { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<MeetingParticipant> MeetingParticipants { get; set; }
    public DbSet<TimeReport> TimeReports { get; set; }
    public DbSet<XPHistory> XPHistories { get; set; }
    public DbSet<UserSecurityLog> UserSecurityLogs { get; set; }
    public DbSet<ReportHistory> ReportHistories { get; set; }
    public DbSet<ReportTemplate> ReportTemplates { get; set; }
    public DbSet<ScheduledReport> ScheduledReports { get; set; }
    public DbSet<CustomReportField> CustomReportFields { get; set; }
    public DbSet<ReportPermission> ReportPermissions { get; set; }
    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Disable cascade delete on all Tenant FK relationships to avoid
        // SQL Server multiple cascade paths with entities that also FK to AspNetUsers.
        foreach (var fk in builder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.PrincipalEntityType.ClrType == typeof(Tenant) && fk.DeleteBehavior == DeleteBehavior.Cascade))
        {
            fk.DeleteBehavior = DeleteBehavior.NoAction;
        }

        // Project configuration
        builder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Description).HasColumnType("nvarchar(max)");
            e.Property(p => p.Status).HasConversion<string>();
            e.Property(p => p.Budget).HasColumnType("decimal(18,2)");
            e.Property(p => p.TotalCost).HasColumnType("decimal(18,2)");
            e.Property(p => p.StartDate).HasColumnType("datetime2");
            e.Property(p => p.EndDate).HasColumnType("datetime2");
            e.Property(p => p.CreatedAt).HasColumnType("datetime2");
            e.Property(p => p.UpdatedAt).HasColumnType("datetime2");
            e.Property(p => p.CreatedBy).IsRequired().HasMaxLength(450);
            e.Property(p => p.UpdatedBy).IsRequired().HasMaxLength(450);
            e.Property(p => p.TenantId).IsRequired();
        });

        // ProjectMember configuration
        builder.Entity<ProjectMember>(e =>
        {
            e.HasKey(pm => pm.Id);
            e.Property(pm => pm.Role).HasConversion<string>();
            e.Property(pm => pm.JoinedAt).HasColumnType("datetime2");
            e.Property(pm => pm.TenantId).IsRequired();
        });

        // Task configuration
        builder.Entity<TaskItem>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).IsRequired().HasMaxLength(200);
            e.Property(t => t.Description).HasColumnType("nvarchar(max)");
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.Priority).HasConversion<string>();
            e.Property(t => t.Deadline).HasColumnType("datetime2");
            e.Property(t => t.CreatedAt).HasColumnType("datetime2");
            e.Property(t => t.AssignedToId).IsRequired(false);
            e.Property(t => t.ProjectId).IsRequired(false);
            e.Property(t => t.SprintId).IsRequired(false);
            e.Property(t => t.TenantId).IsRequired();

            e.HasOne(t => t.AssignedTo)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ApplicationUser - ignore unused Tasks collection
        builder.Entity<ApplicationUser>(e =>
        {
            e.Ignore(u => u.Tasks);
        });

        // Task dependency configuration
        builder.Entity<TaskDependency>(e =>
        {
            e.HasKey(td => td.Id);
            e.Property(td => td.TenantId).IsRequired();
            e.Property(td => td.CreatedAt).HasColumnType("datetime2");

            e.HasOne(td => td.Task)
                .WithMany(t => t.Dependencies)
                .HasForeignKey(td => td.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(td => td.DependsOnTask)
                .WithMany(t => t.DependentOn)
                .HasForeignKey(td => td.DependsOnTaskId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Comment configuration
        builder.Entity<Comment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Content).IsRequired();
            e.Property(c => c.CreatedAt).HasColumnType("datetime2");
            e.Property(c => c.UserId).IsRequired();
            e.Property(c => c.TaskId).IsRequired();
            e.Property(c => c.TenantId).IsRequired();
        });

        // UserProfile configuration
        builder.Entity<UserProfile>(e =>
        {
            e.HasKey(up => up.Id);
            e.Property(up => up.TotalXP).HasColumnType("int");
            e.Property(up => up.CurrentLevel).HasColumnType("int");
            e.Property(up => up.StreakDays).HasColumnType("int");
            e.Property(up => up.LastActivityDate).HasColumnType("datetime2");
            e.Property(up => up.UserId).IsRequired();
            e.Property(up => up.TenantId).IsRequired();
        });

        // Badge configuration
        builder.Entity<Badge>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(100);
            e.Property(b => b.Icon).HasMaxLength(50);
            e.Property(b => b.Description).HasColumnType("nvarchar(max)");
            e.Property(b => b.Criteria).HasColumnType("nvarchar(max)");
            e.Property(b => b.XPReward).HasColumnType("int");
            e.Property(b => b.CreatedAt).HasColumnType("datetime2");
            e.Property(b => b.TenantId).IsRequired();
        });

        // UserBadge configuration
        builder.Entity<UserBadge>(e =>
        {
            e.HasKey(ub => ub.Id);
            e.Property(ub => ub.EarnedAt).HasColumnType("datetime2");
            e.Property(ub => ub.TenantId).IsRequired();
        });


        // TimeReport configuration
        builder.Entity<TimeReport>(e =>
        {
            e.HasKey(tr => tr.Id);
            e.Property(tr => tr.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(tr => tr.TotalHours).HasColumnType("time");
            e.Property(tr => tr.GeneratedAt).HasColumnType("datetime2");
            e.Property(tr => tr.StartDate).HasColumnType("datetime2");
            e.Property(tr => tr.EndDate).HasColumnType("datetime2");
            e.Property(tr => tr.ExpiresAt).HasColumnType("datetime2");
            e.Property(tr => tr.ReportData).HasColumnType("nvarchar(max)");
            e.Property(tr => tr.TenantId).IsRequired();
        });

        // TimeEntry configuration
        builder.Entity<TimeEntry>(e =>
        {
            e.HasKey(te => te.Id);
            e.Property(te => te.Description).HasColumnType("nvarchar(max)");
            e.Property(te => te.StartTime).HasColumnType("datetime2");
            e.Property(te => te.EndTime).HasColumnType("datetime2");
            e.Property(te => te.CreatedAt).HasColumnType("datetime2");
            e.Property(te => te.UserId).IsRequired();
            e.Property(te => te.TaskId).IsRequired();
            e.Property(te => te.TenantId).IsRequired();
        });

        // ActivityLog configuration
        builder.Entity<ActivityLog>(e =>
        {
            e.HasKey(al => al.Id);
            e.Property(al => al.Action).IsRequired().HasMaxLength(100);
            e.Property(al => al.Timestamp).HasColumnType("datetime2");
            e.Property(al => al.ActorId).IsRequired();
            e.Property(al => al.TenantId).IsRequired();
        });


        // Notification configuration
        builder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Message).IsRequired();
            e.Property(n => n.Type).IsRequired().HasMaxLength(50);
            e.Property(n => n.IsRead).HasDefaultValue(false);
            e.Property(n => n.CreatedAt).HasColumnType("datetime2");
            e.Property(n => n.UserId).IsRequired();
            e.Property(n => n.TenantId).IsRequired();
        });

        // Feedback configuration
        builder.Entity<Feedback>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Comment).IsRequired(false);
            e.Property(f => f.Rating).HasColumnType("int");
            e.Property(f => f.CreatedAt).HasColumnType("datetime2");
            e.Property(f => f.TaskId).IsRequired();
            e.Property(f => f.ClientId).IsRequired();
            e.Property(f => f.TenantId).IsRequired();
        });

        // Meeting configuration
        builder.Entity<Meeting>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Title).IsRequired().HasMaxLength(200);
            e.Property(m => m.Description).HasColumnType("nvarchar(max)");
            e.Property(m => m.ScheduledAt).HasColumnType("datetime2");
            e.Property(m => m.StartedAt).HasColumnType("datetime2");
            e.Property(m => m.EndedAt).HasColumnType("datetime2");
            e.Property(m => m.Type).HasConversion<string>();
            e.Property(m => m.Status).HasConversion<string>();
            e.Property(m => m.CreatedAt).HasColumnType("datetime2");
            e.Property(m => m.UpdatedAt).HasColumnType("datetime2");
            e.Property(m => m.CreatedBy).IsRequired().HasMaxLength(450);
            e.Property(m => m.FacilitatedBy).IsRequired(false).HasMaxLength(450);
            e.Property(m => m.TenantId).IsRequired();

            e.HasOne(m => m.CreatedByUser)
                .WithMany()
                .HasForeignKey(m => m.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(m => m.Facilitator)
                .WithMany()
                .HasForeignKey(m => m.FacilitatedBy)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // MeetingNote configuration
        builder.Entity<MeetingNote>(e =>
        {
            e.HasKey(mn => mn.Id);
            e.Property(mn => mn.Content).IsRequired();
            e.Property(mn => mn.Type).HasConversion<string>();
            e.Property(mn => mn.CreatedAt).HasColumnType("datetime2");
            e.Property(mn => mn.UpdatedAt).HasColumnType("datetime2");
            e.Property(mn => mn.IsPublic).HasDefaultValue(true);
            e.Property(mn => mn.AuthorId).IsRequired(false).HasMaxLength(450);
            e.Property(mn => mn.TenantId).IsRequired();
        });

        // StandupNote configuration
        builder.Entity<StandupNote>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Date).HasColumnType("datetime2");
            e.Property(s => s.CreatedAt).HasColumnType("datetime2");
            e.Property(s => s.UpdatedAt).HasColumnType("datetime2");
            e.Property(s => s.UserId).IsRequired();
            e.Property(s => s.ProjectId).IsRequired();
            e.Property(s => s.TenantId).IsRequired();
        });

        // RetrospectiveNote configuration
        builder.Entity<RetrospectiveNote>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Content).IsRequired();
            e.Property(r => r.CreatedAt).HasColumnType("datetime2");
            e.Property(r => r.UpdatedAt).HasColumnType("datetime2");
            e.Property(r => r.AuthorId).IsRequired(false).HasMaxLength(450);
            e.Property(r => r.TenantId).IsRequired();
        });

        // MeetingActionItem configuration
        builder.Entity<MeetingActionItem>(e =>
        {
            e.HasKey(ma => ma.Id);
            e.Property(ma => ma.Title).IsRequired().HasMaxLength(200);
            e.Property(ma => ma.Description).HasColumnType("nvarchar(max)");
            e.Property(ma => ma.AssignedToId).IsRequired(false).HasMaxLength(450);
            e.Property(ma => ma.DueDate).HasColumnType("datetime2");
            e.Property(ma => ma.CreatedAt).HasColumnType("datetime2");
            e.Property(ma => ma.CreatedBy).IsRequired().HasMaxLength(450);
            e.Property(ma => ma.TenantId).IsRequired();

            e.HasOne(ma => ma.CreatedByUser)
                .WithMany()
                .HasForeignKey(ma => ma.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Sprint configuration
        builder.Entity<Sprint>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(200);
            e.Property(s => s.Description).HasColumnType("nvarchar(max)");
            e.Property(s => s.Status).HasConversion<string>();
            e.Property(s => s.StartDate).HasColumnType("datetime2");
            e.Property(s => s.EndDate).HasColumnType("datetime2");
            e.Property(s => s.CreatedAt).HasColumnType("datetime2");
            e.Property(s => s.UpdatedAt).HasColumnType("datetime2");
            e.Property(s => s.CreatedBy).IsRequired().HasMaxLength(450);
            e.Property(s => s.UpdatedBy).IsRequired().HasMaxLength(450);
            e.Property(s => s.TenantId).IsRequired();
        });

        // Indexes for performance optimization
        builder.Entity<TaskItem>().HasIndex(e => new { e.AssignedToId, e.Status });
        builder.Entity<TaskItem>().HasIndex(e => new { e.ProjectId, e.Status });
        builder.Entity<TaskItem>().HasIndex(e => new { e.Deadline, e.Status });
        builder.Entity<Comment>().HasIndex(e => new { e.TaskId, e.CreatedAt });
        builder.Entity<UserProfile>().HasIndex(e => new { e.TotalXP });
        builder.Entity<TimeEntry>().HasIndex(e => new { e.UserId, e.StartTime });
        builder.Entity<TimeEntry>().HasIndex(e => new { e.TaskId, e.StartTime });
    }
}
