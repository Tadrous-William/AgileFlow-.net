using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.Enums;
using AgileTaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ATS = AgileTaskManager.Models.Enums.TaskStatus;

namespace AgileTaskManager.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context     = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var tenantCtx   = services.GetRequiredService<ITenantContext>();

        await WipeAllDataAsync(context);

        // ── Roles ─────────────────────────────────────────────────────
        foreach (var r in new[] { "Admin","TeamLead","Developer","Viewer","Client" })
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));

        // ── Tenant ────────────────────────────────────────────────────
        var tenant = new Tenant { Name = "Agile Corp", IsActive = true };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        int tid = tenant.Id;
        tenantCtx.SetTenant(tid);

        const string PW = "Tad123456!";

        // ── Users ─────────────────────────────────────────────────────
        var userDefs = new[]
        {
            ("Tadrous@gmail.com",  "Tadrous Admin",     "Admin"),
            ("Diaa@gmail.com",     "Diaa Naguib",       "Developer"),
            ("Saad@gmail.com",     "Saad Khalil",       "Client"),
            ("Ahmed@gmail.com",    "Ahmed Hassan",      "Developer"),
            ("Mohamed@gmail.com",  "Mohamed Ali",       "Developer"),
            ("Omar@gmail.com",     "Omar Sherif",       "Developer"),
            ("Ali@gmail.com",      "Ali Mahmoud",       "Developer"),
            ("Hassan@gmail.com",   "Hassan Farouk",     "Developer"),
            ("Youssef@gmail.com",  "Youssef Samir",     "Developer"),
            ("Kareem@gmail.com",   "Kareem Adel",       "Developer"),
            ("Ibrahim@gmail.com",  "Ibrahim Nasser",    "Developer"),
            ("Mahmoud@gmail.com",  "Mahmoud Tarek",     "Developer"),
            ("Tariq@gmail.com",    "Tariq Walid",       "Developer"),
            ("Nabil@gmail.com",    "Nabil Fawzy",       "Developer"),
            ("Samir@gmail.com",    "Samir Hamed",       "Developer"),
            ("Walid@gmail.com",    "Walid Rashid",      "Developer"),
            ("Fadi@gmail.com",     "Fadi Bassem",       "Developer"),
            ("Rami@gmail.com",     "Rami Karim",        "Developer"),
            ("Karim@gmail.com",    "Karim Hani",        "Developer"),
            ("Hani@gmail.com",     "Hani Jamil",        "Developer"),
            ("Client1@gmail.com",  "Layla Mostafa",     "Client"),
            ("Client2@gmail.com",  "Nour Ashraf",       "Client"),
            ("Client3@gmail.com",  "Rania Fouad",       "Client"),
            ("Client4@gmail.com",  "Sherif Gabr",       "Client"),
            ("Client5@gmail.com",  "Mona Zaki",         "Client"),
        };

        var users = new Dictionary<string, ApplicationUser>();
        foreach (var (email, name, role) in userDefs)
        {
            var u = new ApplicationUser
            {
                FullName = name, UserName = email, Email = email, Role = role,
                IsActive = true, TenantId = tid, EmailConfirmed = true,
                Department = role == "Admin" ? "Management" : role == "Client" ? "External" : "Engineering",
                Position   = role == "Admin" ? "System Administrator" : role == "Client" ? "Client Representative" : "Software Developer"
            };
            var res = await userManager.CreateAsync(u, PW);
            if (!res.Succeeded) throw new Exception($"Failed to create {email}: {string.Join(", ", res.Errors.Select(e => e.Description))}");
            await userManager.AddToRoleAsync(u, role);
            users[email] = u;
        }

        var admin   = users["Tadrous@gmail.com"];
        var clients = new[] { users["Saad@gmail.com"], users["Client1@gmail.com"], users["Client2@gmail.com"], users["Client3@gmail.com"], users["Client4@gmail.com"], users["Client5@gmail.com"] };
        var devs    = new[]
        {
            users["Diaa@gmail.com"],    // [0]  4800 XP
            users["Ahmed@gmail.com"],   // [1]  3950
            users["Mohamed@gmail.com"], // [2]  3200
            users["Omar@gmail.com"],    // [3]  2700
            users["Ali@gmail.com"],     // [4]  2300
            users["Hassan@gmail.com"],  // [5]  1900
            users["Youssef@gmail.com"], // [6]  1500
            users["Kareem@gmail.com"],  // [7]  1200
            users["Ibrahim@gmail.com"], // [8]   950
            users["Mahmoud@gmail.com"], // [9]   720
            users["Tariq@gmail.com"],   // [10]  580
            users["Nabil@gmail.com"],   // [11]  430
            users["Samir@gmail.com"],   // [12]  350
            users["Walid@gmail.com"],   // [13]  280
            users["Fadi@gmail.com"],    // [14]  200
            users["Rami@gmail.com"],    // [15]  150
            users["Karim@gmail.com"],   // [16]  120
            users["Hani@gmail.com"],    // [17]   90
            users["Kareem@gmail.com"],  // [18] dup — reuse Kareem
            users["Hassan@gmail.com"],  // [19] dup — reuse Hassan
        };

        // ── Badges ────────────────────────────────────────────────────
        var badges = new List<Badge>
        {
            new() { Name="First Task",       Description="Complete your first task",          Icon="🎯", Category="Tasks",         XPReward=50,  TenantId=tid, Criteria="{\"type\":\"tasksCompleted\",\"count\":1}",  IsActive=true },
            new() { Name="Task Master",      Description="Complete 10 tasks",                 Icon="🏆", Category="Tasks",         XPReward=200, TenantId=tid, Criteria="{\"type\":\"tasksCompleted\",\"count\":10}", IsActive=true },
            new() { Name="Task Champion",    Description="Complete 25 tasks",                 Icon="👑", Category="Tasks",         XPReward=500, TenantId=tid, Criteria="{\"type\":\"tasksCompleted\",\"count\":25}", IsActive=true },
            new() { Name="Commenter",        Description="Post 5 comments",                   Icon="💬", Category="Collaboration", XPReward=75,  TenantId=tid, Criteria="{\"type\":\"commentsPosted\",\"count\":5}",  IsActive=true },
            new() { Name="Conversation Pro", Description="Post 20 comments",                  Icon="🗣️", Category="Collaboration", XPReward=150, TenantId=tid, Criteria="{\"type\":\"commentsPosted\",\"count\":20}", IsActive=true },
            new() { Name="Streak Starter",   Description="Maintain a 3-day streak",           Icon="🔥", Category="Streak",        XPReward=100, TenantId=tid, Criteria="{\"type\":\"streakDays\",\"count\":3}",      IsActive=true },
            new() { Name="On Fire",          Description="Maintain a 7-day streak",           Icon="🌟", Category="Streak",        XPReward=250, TenantId=tid, Criteria="{\"type\":\"streakDays\",\"count\":7}",      IsActive=true },
            new() { Name="Bug Hunter",       Description="Fix 5 reported bugs",               Icon="🐛", Category="Tasks",         XPReward=150, TenantId=tid, Criteria="{\"type\":\"bugsFixed\",\"count\":5}",       IsActive=true },
            new() { Name="Speed Runner",     Description="Complete a task in under 2 hours",  Icon="⚡", Category="Tasks",         XPReward=75,  TenantId=tid, Criteria="{\"type\":\"fastCompletion\"}",              IsActive=true },
            new() { Name="Team Player",      Description="Active on 3 or more projects",      Icon="🤝", Category="Collaboration", XPReward=200, TenantId=tid, Criteria="{\"type\":\"projectsJoined\",\"count\":3}",  IsActive=true },
        };
        context.Badges.AddRange(badges);
        await context.SaveChangesAsync();

        // ── User Profiles ─────────────────────────────────────────────
        int[] xpArr = { 4800, 3950, 3200, 2700, 2300, 1900, 1500, 1200, 950, 720, 580, 430, 350, 280, 200, 150, 120, 90 };
        for (int i = 0; i < devs.Length && i < xpArr.Length; i++)
        {
            int xp = xpArr[i];
            int lv = xp >= 4000 ? 8 : xp >= 3000 ? 7 : xp >= 2000 ? 6 : xp >= 1000 ? 5 : xp >= 500 ? 4 : xp >= 250 ? 3 : xp >= 100 ? 2 : 1;
            context.UserProfiles.Add(new UserProfile
            {
                UserId = devs[i].Id, TenantId = tid,
                TotalXP = xp, CurrentLevel = lv, XPToNextLevel = lv >= 8 ? 10000 : lv * 1000,
                TasksCompleted = xp / 80, TasksCreated = xp / 200, CommentsPosted = xp / 100,
                StreakDays = i < 5 ? 7 : i < 10 ? 3 : 1,
                LastActivityDate = DateTime.UtcNow.AddDays(-(i % 3))
            });
        }
        context.UserProfiles.Add(new UserProfile
        {
            UserId = admin.Id, TenantId = tid,
            TotalXP = 9500, CurrentLevel = 10, XPToNextLevel = 15000,
            TasksCompleted = 120, TasksCreated = 250, CommentsPosted = 95,
            StreakDays = 14, LastActivityDate = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // ── Badge Awards ──────────────────────────────────────────────
        var awards = new (int DevIdx, int BadgeIdx)[]
        {
            (0,0),(1,0),(2,0),(3,0),(4,0),(5,0),(6,0),(7,0),(8,0),  // First Task × 9
            (0,1),(1,1),(2,1),(3,1),(4,1),                           // Task Master × 5
            (0,2),(1,2),                                             // Task Champion × 2
            (0,3),(1,3),(2,3),(3,3),(4,3),(5,3),                    // Commenter × 6
            (0,4),(1,4),                                             // Conversation Pro × 2
            (0,5),(1,5),(2,5),(3,5),(4,5),                          // Streak Starter × 5
            (0,6),(1,6),                                             // On Fire × 2
            (0,7),(1,7),(2,7),                                       // Bug Hunter × 3
            (0,8),(3,8),                                             // Speed Runner × 2
            (0,9),(1,9),(2,9),                                       // Team Player × 3
        };
        int dayOff = 0;
        foreach (var (di, bi) in awards)
        {
            if (di >= devs.Length) continue;
            context.UserBadges.Add(new UserBadge
            {
                UserId = devs[di].Id, BadgeId = badges[bi].Id, TenantId = tid,
                EarnedAt = DateTime.UtcNow.AddDays(-60 + dayOff++),
                XPAwarded = badges[bi].XPReward,
                Name = badges[bi].Name, Description = badges[bi].Description
            });
        }
        await context.SaveChangesAsync();

        // ── Projects ──────────────────────────────────────────────────
        var p1  = new Project { Name="Cloud Migration",              Description="Migrate on-premise infrastructure to AWS — EC2, RDS, S3, CloudFront, Route 53 and full CI/CD pipelines.",                           StartDate=new DateTime(2026,1,15), EndDate=new DateTime(2026,9,30),  Budget=250000, TotalCost=87500,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,1,8),  CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p2  = new Project { Name="Mobile App v2",                Description="Cross-platform React Native rebuild with offline-first support, biometric auth, push notifications and a new design system.",        StartDate=new DateTime(2026,2,1),  EndDate=new DateTime(2026,8,15),  Budget=180000, TotalCost=62000,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,1,25), CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p3  = new Project { Name="Customer Portal",              Description="Self-service portal for clients — support ticketing, SLA tracking, project visibility, PDF/CSV exports and client feedback.",         StartDate=new DateTime(2026,1,20), EndDate=new DateTime(2026,7,30),  Budget=120000, TotalCost=54000,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,1,13), CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p4  = new Project { Name="E-Commerce Redesign",          Description="Full UI/UX overhaul — new checkout flow, recommendation engine, product listing redesign and mobile-responsive payment gateway.",      StartDate=new DateTime(2026,3,1),  EndDate=new DateTime(2026,10,31), Budget=320000, TotalCost=105000, Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,2,20), CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p5  = new Project { Name="Analytics Dashboard",          Description="Real-time BI dashboard with KPI tracking, predictive analytics, custom report builder and automated scheduled email reports.",        StartDate=new DateTime(2026,2,15), EndDate=new DateTime(2026,8,31),  Budget=95000,  TotalCost=38000,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,2,8),  CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p6  = new Project { Name="Security & Compliance",        Description="SOC 2 Type II certification — penetration testing, GDPR data mapping, SIEM integration, 2FA rollout and incident response playbook.",  StartDate=new DateTime(2026,3,15), EndDate=new DateTime(2026,9,15),  Budget=140000, TotalCost=47000,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,3,8),  CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p7  = new Project { Name="DevOps Pipeline Automation",   Description="CI/CD pipelines, container orchestration with Kubernetes, monitoring with Prometheus/Grafana, and infrastructure as code with Terraform.", StartDate=new DateTime(2026,2,10), EndDate=new DateTime(2026,8,30),  Budget=175000, TotalCost=68000,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,2,3),  CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p8  = new Project { Name="AI Chatbot Integration",       Description="NLP-powered conversational AI — sentiment analysis, intent recognition, multi-language support and seamless CRM integration.",          StartDate=new DateTime(2026,3,1),  EndDate=new DateTime(2026,9,30),  Budget=210000, TotalCost=72000,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,2,22), CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p9  = new Project { Name="Data Lake Architecture",       Description="Big data platform — ETL pipelines with Apache Spark, data governance policies, schema registry and real-time streaming with Kafka.",    StartDate=new DateTime(2026,1,25), EndDate=new DateTime(2026,10,15), Budget=280000, TotalCost=93000,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,1,18), CreatedBy=admin.Id, UpdatedBy=admin.Id };
        var p10 = new Project { Name="HR Management System",         Description="Employee onboarding workflows, payroll integration, performance reviews, leave management and org chart visualization.",                 StartDate=new DateTime(2026,2,20), EndDate=new DateTime(2026,9,30),  Budget=160000, TotalCost=55000,  Status="Active", TenantId=tid, CreatedAt=new DateTime(2026,2,12), CreatedBy=admin.Id, UpdatedBy=admin.Id };
        context.Projects.AddRange(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        await context.SaveChangesAsync();

        // ── Project Members ───────────────────────────────────────────
        void M(Project p, ApplicationUser u, ProjectMemberRole r) =>
            context.ProjectMembers.Add(new ProjectMember { ProjectId=p.Id, UserId=u.Id, Role=r, TenantId=tid, JoinedAt=p.StartDate!.Value.AddDays(1) });

        M(p1, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[0],devs[1],devs[2],devs[3],devs[4]}) M(p1, d, ProjectMemberRole.Developer);
        M(p1, clients[0], ProjectMemberRole.Viewer);

        M(p2, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[2],devs[3],devs[4],devs[5],devs[6]}) M(p2, d, ProjectMemberRole.Developer);
        M(p2, clients[1], ProjectMemberRole.Viewer);

        M(p3, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[0],devs[4],devs[5],devs[7]}) M(p3, d, ProjectMemberRole.Developer);
        M(p3, clients[0], ProjectMemberRole.Viewer);
        M(p3, clients[2], ProjectMemberRole.Viewer);

        M(p4, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[6],devs[7],devs[8],devs[9],devs[10]}) M(p4, d, ProjectMemberRole.Developer);
        M(p4, clients[3], ProjectMemberRole.Viewer);

        M(p5, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[11],devs[12],devs[13],devs[14]}) M(p5, d, ProjectMemberRole.Developer);

        M(p6, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[0],devs[15],devs[16],devs[17]}) M(p6, d, ProjectMemberRole.Developer);
        M(p6, clients[4], ProjectMemberRole.Viewer);

        M(p7, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[1],devs[3],devs[8],devs[14]}) M(p7, d, ProjectMemberRole.Developer);
        M(p7, clients[0], ProjectMemberRole.Viewer);

        M(p8, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[5],devs[9],devs[11],devs[13]}) M(p8, d, ProjectMemberRole.Developer);
        M(p8, clients[1], ProjectMemberRole.Viewer);

        M(p9, admin,      ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[2],devs[6],devs[10],devs[12]}) M(p9, d, ProjectMemberRole.Developer);
        M(p9, clients[2], ProjectMemberRole.Viewer);

        M(p10, admin,     ProjectMemberRole.TeamLead);
        foreach (var d in new[]{devs[4],devs[7],devs[15],devs[16]}) M(p10, d, ProjectMemberRole.Developer);
        M(p10, clients[3], ProjectMemberRole.Viewer);
        await context.SaveChangesAsync();

        // ── Sprints (5 per project) ───────────────────────────────────
        Sprint Sp(Project p, int n, SprintStatus st) => new()
        {
            Name=$"Sprint {n}", ProjectId=p.Id, TenantId=tid,
            StartDate=p.StartDate!.Value.AddDays((n-1)*14),
            EndDate  =p.StartDate!.Value.AddDays((n-1)*14+13),
            Status=st, CreatedBy=admin.Id, UpdatedBy=admin.Id, CreatedAt=p.StartDate!.Value
        };

        // Project 1 – Cloud Migration
        var sp1a=Sp(p1,1,SprintStatus.Completed); var sp1b=Sp(p1,2,SprintStatus.Completed); var sp1c=Sp(p1,3,SprintStatus.Completed);
        var sp1d=Sp(p1,4,SprintStatus.Active);    var sp1e=Sp(p1,5,SprintStatus.Planned);
        // Project 2 – Mobile App v2
        var sp2a=Sp(p2,1,SprintStatus.Completed); var sp2b=Sp(p2,2,SprintStatus.Completed); var sp2c=Sp(p2,3,SprintStatus.Completed);
        var sp2d=Sp(p2,4,SprintStatus.Active);    var sp2e=Sp(p2,5,SprintStatus.Planned);
        // Project 3 – Customer Portal
        var sp3a=Sp(p3,1,SprintStatus.Completed); var sp3b=Sp(p3,2,SprintStatus.Completed); var sp3c=Sp(p3,3,SprintStatus.Completed);
        var sp3d=Sp(p3,4,SprintStatus.Active);    var sp3e=Sp(p3,5,SprintStatus.Planned);
        // Project 4 – E-Commerce Redesign
        var sp4a=Sp(p4,1,SprintStatus.Completed); var sp4b=Sp(p4,2,SprintStatus.Completed); var sp4c=Sp(p4,3,SprintStatus.Completed);
        var sp4d=Sp(p4,4,SprintStatus.Active);    var sp4e=Sp(p4,5,SprintStatus.Planned);
        // Project 5 – Analytics Dashboard
        var sp5a=Sp(p5,1,SprintStatus.Completed); var sp5b=Sp(p5,2,SprintStatus.Completed); var sp5c=Sp(p5,3,SprintStatus.Completed);
        var sp5d=Sp(p5,4,SprintStatus.Active);    var sp5e=Sp(p5,5,SprintStatus.Planned);
        // Project 6 – Security & Compliance
        var sp6a=Sp(p6,1,SprintStatus.Completed); var sp6b=Sp(p6,2,SprintStatus.Completed); var sp6c=Sp(p6,3,SprintStatus.Completed);
        var sp6d=Sp(p6,4,SprintStatus.Active);    var sp6e=Sp(p6,5,SprintStatus.Planned);
        // Project 7 – DevOps Pipeline Automation
        var sp7a=Sp(p7,1,SprintStatus.Completed); var sp7b=Sp(p7,2,SprintStatus.Completed); var sp7c=Sp(p7,3,SprintStatus.Completed);
        var sp7d=Sp(p7,4,SprintStatus.Active);    var sp7e=Sp(p7,5,SprintStatus.Planned);
        // Project 8 – AI Chatbot Integration
        var sp8a=Sp(p8,1,SprintStatus.Completed); var sp8b=Sp(p8,2,SprintStatus.Completed); var sp8c=Sp(p8,3,SprintStatus.Completed);
        var sp8d=Sp(p8,4,SprintStatus.Active);    var sp8e=Sp(p8,5,SprintStatus.Planned);
        // Project 9 – Data Lake Architecture
        var sp9a=Sp(p9,1,SprintStatus.Completed); var sp9b=Sp(p9,2,SprintStatus.Completed); var sp9c=Sp(p9,3,SprintStatus.Completed);
        var sp9d=Sp(p9,4,SprintStatus.Active);    var sp9e=Sp(p9,5,SprintStatus.Planned);
        // Project 10 – HR Management System
        var sp10a=Sp(p10,1,SprintStatus.Completed); var sp10b=Sp(p10,2,SprintStatus.Completed); var sp10c=Sp(p10,3,SprintStatus.Completed);
        var sp10d=Sp(p10,4,SprintStatus.Active);    var sp10e=Sp(p10,5,SprintStatus.Planned);

        context.Sprints.AddRange(
            sp1a,sp1b,sp1c,sp1d,sp1e, sp2a,sp2b,sp2c,sp2d,sp2e,
            sp3a,sp3b,sp3c,sp3d,sp3e, sp4a,sp4b,sp4c,sp4d,sp4e,
            sp5a,sp5b,sp5c,sp5d,sp5e, sp6a,sp6b,sp6c,sp6d,sp6e,
            sp7a,sp7b,sp7c,sp7d,sp7e, sp8a,sp8b,sp8c,sp8d,sp8e,
            sp9a,sp9b,sp9c,sp9d,sp9e, sp10a,sp10b,sp10c,sp10d,sp10e);
        await context.SaveChangesAsync();

        // ════════════════════════════════════════════════════════════════
        // PART 2 ─ Tasks, Comments, Time Entries, Meetings, etc.
        // ════════════════════════════════════════════════════════════════
        await SeedPart2Async(context, tid, admin, devs, clients, badges,
            p1,p2,p3,p4,p5,p6,p7,p8,p9,p10,
            sp1a,sp1b,sp1c,sp1d,sp1e,
            sp2a,sp2b,sp2c,sp2d,sp2e,
            sp3a,sp3b,sp3c,sp3d,sp3e,
            sp4a,sp4b,sp4c,sp4d,sp4e,
            sp5a,sp5b,sp5c,sp5d,sp5e,
            sp6a,sp6b,sp6c,sp6d,sp6e,
            sp7a,sp7b,sp7c,sp7d,sp7e,
            sp8a,sp8b,sp8c,sp8d,sp8e,
            sp9a,sp9b,sp9c,sp9d,sp9e,
            sp10a,sp10b,sp10c,sp10d,sp10e);
    }

    // ── Part 2 — Full seed ────────────────────────────────────────────
    private static async Task SeedPart2Async(
        ApplicationDbContext db, int tid,
        ApplicationUser admin, ApplicationUser[] devs, ApplicationUser[] clients, List<Badge> badges,
        Project p1, Project p2, Project p3, Project p4, Project p5, Project p6,
        Project p7, Project p8, Project p9, Project p10,
        Sprint sp1a, Sprint sp1b, Sprint sp1c, Sprint sp1d, Sprint sp1e,
        Sprint sp2a, Sprint sp2b, Sprint sp2c, Sprint sp2d, Sprint sp2e,
        Sprint sp3a, Sprint sp3b, Sprint sp3c, Sprint sp3d, Sprint sp3e,
        Sprint sp4a, Sprint sp4b, Sprint sp4c, Sprint sp4d, Sprint sp4e,
        Sprint sp5a, Sprint sp5b, Sprint sp5c, Sprint sp5d, Sprint sp5e,
        Sprint sp6a, Sprint sp6b, Sprint sp6c, Sprint sp6d, Sprint sp6e,
        Sprint sp7a, Sprint sp7b, Sprint sp7c, Sprint sp7d, Sprint sp7e,
        Sprint sp8a, Sprint sp8b, Sprint sp8c, Sprint sp8d, Sprint sp8e,
        Sprint sp9a, Sprint sp9b, Sprint sp9c, Sprint sp9d, Sprint sp9e,
        Sprint sp10a, Sprint sp10b, Sprint sp10c, Sprint sp10d, Sprint sp10e)
    {
        // ── Helper shortcuts ──────────────────────────────────────────
        var now = DateTime.UtcNow;

        // ─────────────────────────────────────────────────────────────
        // TASKS  (~130 tasks)
        // ─────────────────────────────────────────────────────────────
        TaskItem T(string title, string desc, Project p, Sprint sp, ApplicationUser? assignee,
                   ATS status, TaskPriority priority, int daysAgo, int daysUntilDeadline)
            => new TaskItem
            {
                Title       = title,
                Description = desc,
                ProjectId   = p.Id,
                SprintId    = sp.Id,
                AssignedToId= assignee?.Id,
                Status      = status,
                Priority    = priority,
                TenantId    = tid,
                CreatedAt   = now.AddDays(-daysAgo),
                StartDate   = now.AddDays(-daysAgo + 1),
                Deadline    = now.AddDays(daysUntilDeadline),
                CompletedAt = status == ATS.Done ? now.AddDays(-daysAgo + 7) : null
            };

        // ── Project 1 – Cloud Migration (14 tasks) ───────────────────
        var t1_1  = T("Set up AWS VPC & subnets",        "Configure VPC with public/private subnets across 3 AZs.",          p1, sp1a, devs[0], ATS.Done,       TaskPriority.High,     95, -80);
        var t1_2  = T("Configure IAM roles & policies",  "Least-privilege IAM roles for EC2, RDS, Lambda, S3.",              p1, sp1a, devs[1], ATS.Done,       TaskPriority.High,     93, -78);
        var t1_3  = T("Migrate PostgreSQL to RDS",        "Lift-and-shift existing Postgres DB with DMS.",                    p1, sp1a, devs[2], ATS.Done,       TaskPriority.High,     90, -75);
        var t1_4  = T("Set up S3 buckets & lifecycle",   "Create versioned S3 buckets, enable lifecycle to Glacier.",         p1, sp1b, devs[0], ATS.Done,       TaskPriority.Medium,   80, -65);
        var t1_5  = T("Deploy EC2 auto-scaling group",   "Launch template, ASG with min 2 / max 10, ALB target group.",       p1, sp1b, devs[1], ATS.Done,       TaskPriority.High,     78, -63);
        var t1_6  = T("CloudFront CDN distribution",     "Configure CDN with custom domain, SSL cert, WAF rules.",            p1, sp1c, devs[2], ATS.Done,       TaskPriority.Medium,   65, -50);
        var t1_7  = T("Route 53 DNS migration",          "Migrate DNS records with zero-downtime weighted routing.",          p1, sp1c, devs[3], ATS.Done,       TaskPriority.High,     63, -48);
        var t1_8  = T("CI/CD pipeline – GitHub Actions", "Build, test, push Docker image, deploy to ECS Fargate.",           p1, sp1c, devs[4], ATS.Done,       TaskPriority.High,     60, -45);
        var t1_9  = T("Set up CloudWatch dashboards",    "CPU, memory, request latency, error rate alarms.",                  p1, sp1d, devs[0], ATS.InProgress, TaskPriority.Medium,   20, 5);
        var t1_10 = T("Cost optimisation review",        "Right-size instances, Reserved Instances plan, Savings Plans.",     p1, sp1d, devs[1], ATS.InProgress, TaskPriority.Low,      18, 8);
        var t1_11 = T("VPN gateway configuration",       "Site-to-site VPN between on-prem DC and AWS VPC.",                 p1, sp1d, devs[3], ATS.ToDo,       TaskPriority.High,     14, 10);
        var t1_12 = T("ECS Fargate service mesh",        "Service discovery with Cloud Map, Envoy sidecar proxies.",         p1, sp1d, devs[4], ATS.Testing,   TaskPriority.Medium,   16, 7);
        var t1_13 = T("Disaster recovery plan",          "Multi-region failover with Route 53 health checks and RDS replica.",p1, sp1e, devs[0], ATS.ToDo,       TaskPriority.High, 8,  20);
        var t1_14 = T("AWS Well-Architected review",     "Complete review across all five pillars with remediation tracker.", p1, sp1e, devs[2], ATS.ToDo,       TaskPriority.Medium,   6,  25);

        // ── Project 2 – Mobile App v2 (13 tasks) ─────────────────────
        var t2_1  = T("React Native project scaffold",    "Expo + TypeScript + NativeWind + React Navigation setup.",         p2, sp2a, devs[2], ATS.Done,       TaskPriority.High,     92, -77);
        var t2_2  = T("Design system tokens",             "Colors, typography, spacing, icon library.",                       p2, sp2a, devs[3], ATS.Done,       TaskPriority.Medium,   90, -75);
        var t2_3  = T("Biometric authentication",         "FaceID / fingerprint via expo-local-authentication.",             p2, sp2b, devs[4], ATS.Done,       TaskPriority.High,     78, -63);
        var t2_4  = T("Offline-first sync engine",        "SQLite local store + conflict resolution with server.",            p2, sp2b, devs[5], ATS.Done,       TaskPriority.High,     76, -61);
        var t2_5  = T("Push notifications – FCM/APNs",   "Firebase FCM for Android, APNS for iOS, deep-link routing.",       p2, sp2c, devs[6], ATS.Done,       TaskPriority.High,     63, -48);
        var t2_6  = T("Home screen widgets",              "iOS 16 & Android 12 home screen widgets with live data.",          p2, sp2c, devs[4], ATS.Done,       TaskPriority.Medium,   60, -45);
        var t2_7  = T("Dark mode implementation",         "System-aware dark mode with custom theme provider.",               p2, sp2c, devs[3], ATS.Done,       TaskPriority.Medium,   58, -43);
        var t2_8  = T("App performance profiling",        "React Native Flipper profiling, Hermes bundle optimisation.",      p2, sp2d, devs[5], ATS.InProgress, TaskPriority.High,     18, 6);
        var t2_9  = T("Crash reporting – Sentry",         "Sentry SDK with breadcrumbs, source maps, release tracking.",     p2, sp2d, devs[6], ATS.InProgress, TaskPriority.Medium,   16, 8);
        var t2_10 = T("In-app purchase integration",      "RevenueCat SDK for iOS/Android subscriptions.",                   p2, sp2d, devs[2], ATS.ToDo,    TaskPriority.High,     14, 10);
        var t2_11 = T("Accessibility audit (WCAG)",       "VoiceOver / TalkBack testing, contrast ratios, focus order.",     p2, sp2d, devs[4], ATS.Testing,   TaskPriority.Medium,   12, 12);
        var t2_12 = T("App Store submission prep",        "Screenshots, descriptions, privacy policy, review cycle.",         p2, sp2e, devs[2], ATS.ToDo,       TaskPriority.High,     8,  22);
        var t2_13 = T("Deep linking & universal links",   "Branch.io integration for deferred deep links.",                  p2, sp2e, devs[3], ATS.ToDo,       TaskPriority.Medium,   6,  25);

        // ── Project 3 – Customer Portal (13 tasks) ───────────────────
        var t3_1  = T("Support ticket system",            "Create, assign, escalate tickets with SLA timer.",                 p3, sp3a, devs[0], ATS.Done,       TaskPriority.High,     94, -79);
        var t3_2  = T("SLA tracking dashboard",           "Real-time SLA breaches, response time heatmap.",                  p3, sp3a, devs[4], ATS.Done,       TaskPriority.Medium,   90, -75);
        var t3_3  = T("Knowledge base module",            "Searchable FAQ with categories, tags and rich-text editor.",      p3, sp3b, devs[5], ATS.Done,       TaskPriority.Medium,   78, -63);
        var t3_4  = T("PDF/CSV export engine",            "Async export pipeline with presigned S3 download URL.",            p3, sp3b, devs[5], ATS.Done,       TaskPriority.Medium,   75, -60);
        var t3_5  = T("Client feedback surveys",          "Post-ticket CSAT survey with automated follow-ups.",              p3, sp3c, devs[7], ATS.Done,       TaskPriority.Low,      63, -48);
        var t3_6  = T("Notification centre",              "In-app and email notifications for ticket updates.",              p3, sp3c, devs[0], ATS.Done,       TaskPriority.Medium,   60, -45);
        var t3_7  = T("Role-based portal access",         "Client vs Admin roles with fine-grained page permissions.",       p3, sp3d, devs[4], ATS.InProgress, TaskPriority.High,     18, 6);
        var t3_8  = T("Ticket analytics & trends",        "Charts for ticket volume, avg resolution time, SLA compliance.",  p3, sp3d, devs[5], ATS.InProgress, TaskPriority.Medium,   15, 9);
        var t3_9  = T("Live chat widget",                 "WebSocket-based real-time chat with agent availability status.",   p3, sp3d, devs[7], ATS.ToDo,       TaskPriority.High,     12, 12);
        var t3_10 = T("Multi-language support (i18n)",    "Arabic, English, French UI translations with RTL support.",        p3, sp3d, devs[0], ATS.Testing,   TaskPriority.Medium,   14, 8);
        var t3_11 = T("Customer satisfaction reports",     "Monthly CSAT/NPS report generation with trend charts.",           p3, sp3e, devs[4], ATS.ToDo,       TaskPriority.Low,      8,  20);
        var t3_12 = T("Audit trail for ticket changes",   "Full history of status, assignee, and priority changes.",         p3, sp3e, devs[5], ATS.ToDo,       TaskPriority.Medium,   6,  24);
        var t3_13 = T("Email-to-ticket integration",      "Parse incoming emails and auto-create support tickets.",          p3, sp3e, devs[7], ATS.ToDo,       TaskPriority.High,     5,  26);

        // ── Project 4 – E-Commerce Redesign (13 tasks) ───────────────
        var t4_1  = T("New checkout flow UX",             "3-step checkout: cart, shipping, payment.",                         p4, sp4a, devs[6], ATS.Done,       TaskPriority.High,     85, -70);
        var t4_2  = T("Recommendation engine",            "Collaborative filtering with ML.NET product suggestions.",        p4, sp4a, devs[7], ATS.Done,       TaskPriority.High,     82, -67);
        var t4_3  = T("Product listing redesign",         "Lazy-load grid, filters sidebar, instant search with Algolia.",   p4, sp4b, devs[8], ATS.Done,       TaskPriority.High,     72, -57);
        var t4_4  = T("Stripe payment gateway v2",        "SCA-compliant Stripe Elements + Apple/Google Pay.",               p4, sp4b, devs[9], ATS.Done,       TaskPriority.High,     70, -55);
        var t4_5  = T("Mobile responsive overhaul",       "CSS Grid to Container Queries, tested on 12 devices.",            p4, sp4c, devs[10],ATS.Done,       TaskPriority.Medium,   60, -45);
        var t4_6  = T("Wishlist & saved items",           "Persistent wishlist with share functionality.",                    p4, sp4c, devs[6], ATS.Done,       TaskPriority.Low,      58, -43);
        var t4_7  = T("Order tracking real-time",         "WebSocket-based live order tracking with map integration.",       p4, sp4c, devs[8], ATS.Done,       TaskPriority.Medium,   55, -40);
        var t4_8  = T("Performance – Core Web Vitals",    "Target LCP < 2.5s, INP < 200ms, CLS < 0.1.",                     p4, sp4d, devs[9], ATS.InProgress, TaskPriority.High,     18, 6);
        var t4_9  = T("A/B testing framework",            "Feature flags with LaunchDarkly + analytics integration.",        p4, sp4d, devs[10],ATS.InProgress, TaskPriority.Medium,   15, 9);
        var t4_10 = T("SEO optimisation pass",            "Structured data, sitemap, meta tags, canonical URLs.",            p4, sp4d, devs[7], ATS.Testing,   TaskPriority.Medium,   14, 8);
        var t4_11 = T("Customer reviews & ratings",       "Star rating, photo reviews, verified purchase badges.",           p4, sp4d, devs[6], ATS.ToDo,    TaskPriority.Low,      12, 12);
        var t4_12 = T("Inventory management sync",        "Real-time stock sync with warehouse management system.",          p4, sp4e, devs[8], ATS.ToDo,       TaskPriority.High,     8,  20);
        var t4_13 = T("Multi-currency support",           "Dynamic currency conversion with live exchange rates.",            p4, sp4e, devs[9], ATS.ToDo,       TaskPriority.Medium,   6,  24);

        // ── Project 5 – Analytics Dashboard (13 tasks) ───────────────
        var t5_1  = T("KPI tracking widgets",             "Burn-down, velocity, cycle time, throughput widgets.",             p5, sp5a, devs[11],ATS.Done,       TaskPriority.High,     90, -75);
        var t5_2  = T("Predictive analytics module",      "Linear regression forecast for sprint velocity.",                  p5, sp5a, devs[12],ATS.Done,       TaskPriority.High,     88, -73);
        var t5_3  = T("Real-time data pipeline",          "SignalR WebSocket for live dashboard updates.",                   p5, sp5b, devs[13],ATS.Done,       TaskPriority.High,     75, -60);
        var t5_4  = T("Custom report builder",            "Drag-and-drop report designer with saved templates.",             p5, sp5b, devs[14],ATS.Done,       TaskPriority.Medium,   72, -57);
        var t5_5  = T("Scheduled email reports",          "Cron-triggered report generation + SendGrid delivery.",           p5, sp5c, devs[11],ATS.Done,       TaskPriority.Medium,   60, -45);
        var t5_6  = T("Data export – Excel/CSV/PDF",      "Multi-format export with chart images embedded.",                 p5, sp5c, devs[12],ATS.Done,       TaskPriority.Low,      58, -43);
        var t5_7  = T("Dashboard theming engine",         "Custom color schemes, dark mode, white-label support.",           p5, sp5c, devs[13],ATS.Done,       TaskPriority.Low,      55, -40);
        var t5_8  = T("Drill-down chart interactions",    "Click-through from summary to detailed data views.",             p5, sp5d, devs[14],ATS.InProgress, TaskPriority.Medium,   18, 6);
        var t5_9  = T("User activity heatmap",            "Calendar heatmap showing daily activity intensity.",              p5, sp5d, devs[11],ATS.InProgress, TaskPriority.Medium,   15, 9);
        var t5_10 = T("Anomaly detection alerts",         "Statistical anomaly detection with Slack/Teams notifications.",   p5, sp5d, devs[12],ATS.ToDo,       TaskPriority.High,     12, 14);
        var t5_11 = T("Embedded analytics API",           "REST API for embedding charts in third-party apps.",             p5, sp5e, devs[13],ATS.ToDo,       TaskPriority.Medium,   8,  20);
        var t5_12 = T("Data retention policies",          "Configurable retention with auto-archival to cold storage.",      p5, sp5e, devs[14],ATS.ToDo,       TaskPriority.Low,      6,  24);
        var t5_13 = T("Role-based dashboard access",      "Admin vs viewer permissions for sensitive KPI data.",             p5, sp5e, devs[11],ATS.ToDo,       TaskPriority.High,     5,  26);

        // ── Project 6 – Security & Compliance (13 tasks) ─────────────
        var t6_1  = T("Penetration testing",              "External pen test + report remediation plan.",                     p6, sp6a, devs[0], ATS.Done,       TaskPriority.High, 88, -73);
        var t6_2  = T("GDPR data mapping",                "PII audit, data flow diagrams, consent management.",              p6, sp6a, devs[15],ATS.Done,       TaskPriority.High,     85, -70);
        var t6_3  = T("Vulnerability scanner setup",      "Nessus/Qualys scheduled scans with auto-ticketing.",             p6, sp6b, devs[16],ATS.Done,       TaskPriority.High,     72, -57);
        var t6_4  = T("SIEM integration – Splunk",        "Log forwarding, correlation rules, alert playbooks.",             p6, sp6b, devs[16],ATS.Done,       TaskPriority.High,     70, -55);
        var t6_5  = T("2FA rollout – TOTP + SMS",         "Google Authenticator + Twilio SMS OTP for all users.",            p6, sp6c, devs[0], ATS.Done,       TaskPriority.High,     60, -45);
        var t6_6  = T("Security awareness training",      "Phishing simulation and e-learning module deployment.",           p6, sp6c, devs[17],ATS.Done,       TaskPriority.Medium,   58, -43);
        var t6_7  = T("API rate limiting & throttling",   "Redis-backed rate limiter with IP and user-based quotas.",        p6, sp6c, devs[15],ATS.Done,       TaskPriority.Medium,   55, -40);
        var t6_8  = T("Incident response playbook",       "Runbooks for data breach, DDoS, ransomware scenarios.",           p6, sp6d, devs[17],ATS.InProgress, TaskPriority.High,     18, 6);
        var t6_9  = T("SOC 2 Type II audit prep",         "Evidence collection, control mapping, auditor liaison.",          p6, sp6d, devs[15],ATS.InProgress, TaskPriority.High, 16, 8);
        var t6_10 = T("Data encryption at rest",          "AES-256 encryption for database and file storage.",               p6, sp6d, devs[0], ATS.Testing,   TaskPriority.High,     14, 10);
        var t6_11 = T("DLP policy implementation",        "Data loss prevention rules for email and cloud storage.",         p6, sp6d, devs[16],ATS.ToDo,       TaskPriority.Medium,   10, 14);
        var t6_12 = T("Third-party vendor risk assessment","Security questionnaires and compliance checks for vendors.",     p6, sp6e, devs[15],ATS.ToDo,       TaskPriority.High,     8,  20);
        var t6_13 = T("Zero-trust network architecture",  "Micro-segmentation, identity-aware proxy, continuous verification.",p6, sp6e, devs[0], ATS.ToDo,      TaskPriority.High, 6,  25);

        // ── Project 7 – DevOps Pipeline Automation (13 tasks) ────────
        var t7_1  = T("Terraform IaC foundation",         "AWS provider config, state backend in S3, modules structure.",     p7, sp7a, devs[1], ATS.Done,       TaskPriority.High,     90, -75);
        var t7_2  = T("Kubernetes cluster setup (EKS)",   "Managed node groups, RBAC, cluster autoscaler.",                  p7, sp7a, devs[3], ATS.Done,       TaskPriority.High,     88, -73);
        var t7_3  = T("Helm chart library",               "Reusable Helm charts for microservices deployment.",              p7, sp7b, devs[8], ATS.Done,       TaskPriority.Medium,   75, -60);
        var t7_4  = T("GitOps with ArgoCD",               "Automated deployments from Git using ArgoCD sync.",               p7, sp7b, devs[14],ATS.Done,       TaskPriority.High,     72, -57);
        var t7_5  = T("Prometheus monitoring stack",       "Prometheus + Grafana + Alertmanager for k8s metrics.",            p7, sp7c, devs[1], ATS.Done,       TaskPriority.High,     60, -45);
        var t7_6  = T("Log aggregation – EFK stack",      "Elasticsearch, Fluentd, Kibana for centralized logging.",         p7, sp7c, devs[3], ATS.Done,       TaskPriority.Medium,   58, -43);
        var t7_7  = T("Container image scanning",         "Trivy + Snyk in CI pipeline for CVE detection.",                  p7, sp7c, devs[8], ATS.Done,       TaskPriority.High,     55, -40);
        var t7_8  = T("Blue-green deployment pipeline",   "Zero-downtime deployments with automated rollback.",              p7, sp7d, devs[14],ATS.InProgress, TaskPriority.High,     18, 6);
        var t7_9  = T("Canary release automation",        "Gradual traffic shift: 5% -> 25% -> 50% -> 100%.",               p7, sp7d, devs[1], ATS.InProgress, TaskPriority.Medium,   15, 9);
        var t7_10 = T("Secrets management – Vault",       "HashiCorp Vault with dynamic secrets and auto-rotation.",         p7, sp7d, devs[3], ATS.ToDo,       TaskPriority.High, 12, 12);
        var t7_11 = T("Cost monitoring dashboards",       "AWS Cost Explorer + custom Grafana dashboards.",                  p7, sp7d, devs[8], ATS.Testing,   TaskPriority.Low,      14, 8);
        var t7_12 = T("Chaos engineering framework",      "Gremlin/Litmus integration for resilience testing.",              p7, sp7e, devs[14],ATS.ToDo,       TaskPriority.Medium,   8,  20);
        var t7_13 = T("Developer self-service portal",    "Internal portal for env provisioning and pipeline management.",   p7, sp7e, devs[1], ATS.ToDo,       TaskPriority.Low,      6,  25);

        // ── Project 8 – AI Chatbot Integration (13 tasks) ────────────
        var t8_1  = T("NLP model selection & training",   "Fine-tune GPT model on customer support corpus.",                 p8, sp8a, devs[5], ATS.Done,       TaskPriority.High, 85, -70);
        var t8_2  = T("Intent recognition pipeline",      "Custom intent classifier with 95%+ accuracy target.",             p8, sp8a, devs[9], ATS.Done,       TaskPriority.High,     82, -67);
        var t8_3  = T("Conversation flow designer",       "Visual dialog builder with branching and fallback logic.",        p8, sp8b, devs[11],ATS.Done,       TaskPriority.High,     72, -57);
        var t8_4  = T("Sentiment analysis module",        "Real-time sentiment scoring with escalation triggers.",           p8, sp8b, devs[13],ATS.Done,       TaskPriority.Medium,   70, -55);
        var t8_5  = T("Multi-language NLP support",       "Arabic, English, French with language detection.",                p8, sp8c, devs[5], ATS.Done,       TaskPriority.High,     60, -45);
        var t8_6  = T("CRM integration – Salesforce",     "Bi-directional sync with Salesforce cases and contacts.",         p8, sp8c, devs[9], ATS.Done,       TaskPriority.Medium,   58, -43);
        var t8_7  = T("Chat widget – WebSocket",          "Embeddable chat widget with typing indicators and file sharing.", p8, sp8c, devs[11],ATS.Done,       TaskPriority.High,     55, -40);
        var t8_8  = T("Agent handoff workflow",           "Seamless transfer from bot to human agent with context.",         p8, sp8d, devs[13],ATS.InProgress, TaskPriority.High,     18, 6);
        var t8_9  = T("Chatbot analytics dashboard",     "Conversation metrics, CSAT, containment rate, top intents.",      p8, sp8d, devs[5], ATS.InProgress, TaskPriority.Medium,   15, 9);
        var t8_10 = T("Knowledge base integration",       "Auto-suggest FAQ articles during conversation.",                  p8, sp8d, devs[9], ATS.ToDo,       TaskPriority.Medium,   12, 12);
        var t8_11 = T("Voice channel support",            "Speech-to-text and text-to-speech for voice interactions.",       p8, sp8d, devs[11],ATS.ToDo,    TaskPriority.High,     10, 14);
        var t8_12 = T("Chatbot A/B testing",              "Test different conversation flows and measure outcomes.",         p8, sp8e, devs[13],ATS.ToDo,       TaskPriority.Low,      8,  20);
        var t8_13 = T("GDPR compliance for chat data",    "Data retention, right to erasure, anonymization of transcripts.",p8, sp8e, devs[5], ATS.ToDo,       TaskPriority.High,     6,  25);

        // ── Project 9 – Data Lake Architecture (13 tasks) ────────────
        var t9_1  = T("S3 data lake foundation",          "Raw/cleaned/curated zones with IAM and bucket policies.",         p9, sp9a, devs[2], ATS.Done,       TaskPriority.High,     92, -77);
        var t9_2  = T("Apache Spark ETL pipelines",       "PySpark jobs for data transformation and enrichment.",            p9, sp9a, devs[6], ATS.Done,       TaskPriority.High,     90, -75);
        var t9_3  = T("Kafka streaming platform",         "Multi-broker Kafka cluster with Schema Registry.",               p9, sp9b, devs[10],ATS.Done,       TaskPriority.High, 78, -63);
        var t9_4  = T("Data catalog – AWS Glue",          "Automated crawlers, schema discovery, data lineage.",             p9, sp9b, devs[12],ATS.Done,       TaskPriority.Medium,   75, -60);
        var t9_5  = T("Data quality framework",           "Great Expectations for data validation and profiling.",           p9, sp9c, devs[2], ATS.Done,       TaskPriority.High,     63, -48);
        var t9_6  = T("Delta Lake implementation",        "ACID transactions on data lake with time travel.",               p9, sp9c, devs[6], ATS.Done,       TaskPriority.High,     60, -45);
        var t9_7  = T("Athena query optimization",        "Partitioning, columnar formats, query cost optimization.",        p9, sp9c, devs[10],ATS.Done,       TaskPriority.Medium,   58, -43);
        var t9_8  = T("Real-time CDC pipeline",           "Change data capture from PostgreSQL to Kafka.",                   p9, sp9d, devs[12],ATS.InProgress, TaskPriority.High,     18, 6);
        var t9_9  = T("Data governance policies",         "Access controls, data classification, PII tagging.",              p9, sp9d, devs[2], ATS.InProgress, TaskPriority.High, 15, 9);
        var t9_10 = T("ML feature store",                 "Feast feature store for model training and serving.",             p9, sp9d, devs[6], ATS.Testing,   TaskPriority.Medium,   14, 8);
        var t9_11 = T("Data mesh architecture",           "Domain-oriented data ownership and self-serve platform.",         p9, sp9d, devs[10],ATS.ToDo,       TaskPriority.High,     12, 14);
        var t9_12 = T("Cost optimization – storage tiers","S3 Intelligent-Tiering, lifecycle policies, compression.",        p9, sp9e, devs[12],ATS.ToDo,       TaskPriority.Low,      8,  20);
        var t9_13 = T("Data observability platform",      "Monte Carlo / Great Expectations for data health monitoring.",    p9, sp9e, devs[2], ATS.ToDo,       TaskPriority.Medium,   6,  25);

        // ── Project 10 – HR Management System (13 tasks) ─────────────
        var t10_1  = T("Employee onboarding workflow",    "Multi-step onboarding with document upload and e-signatures.",    p10, sp10a, devs[4], ATS.Done,       TaskPriority.High,     88, -73);
        var t10_2  = T("Payroll integration – ADP",       "Bi-directional sync with ADP for salary and deductions.",        p10, sp10a, devs[7], ATS.Done,       TaskPriority.High, 85, -70);
        var t10_3  = T("Performance review module",       "360-degree feedback, goal tracking, competency matrix.",          p10, sp10b, devs[15],ATS.Done,       TaskPriority.High,     75, -60);
        var t10_4  = T("Leave management system",         "Leave requests, approval workflow, balance tracking.",            p10, sp10b, devs[16],ATS.Done,       TaskPriority.Medium,   72, -57);
        var t10_5  = T("Org chart visualization",         "Interactive org chart with drag-and-drop restructuring.",         p10, sp10c, devs[4], ATS.Done,       TaskPriority.Medium,   60, -45);
        var t10_6  = T("Employee self-service portal",    "Profile updates, payslip downloads, tax forms.",                 p10, sp10c, devs[7], ATS.Done,       TaskPriority.High,     58, -43);
        var t10_7  = T("Training & development tracker",  "Course catalog, certifications, skill gap analysis.",            p10, sp10c, devs[15],ATS.Done,       TaskPriority.Medium,   55, -40);
        var t10_8  = T("Benefits enrollment engine",      "Annual enrollment wizard with plan comparison tool.",             p10, sp10d, devs[16],ATS.InProgress, TaskPriority.High,     18, 6);
        var t10_9  = T("Time & attendance system",        "Clock in/out with geofencing and overtime calculations.",         p10, sp10d, devs[4], ATS.InProgress, TaskPriority.Medium,   15, 9);
        var t10_10 = T("Recruitment pipeline (ATS)",      "Job postings, applicant tracking, interview scheduling.",         p10, sp10d, devs[7], ATS.ToDo,       TaskPriority.High,     12, 12);
        var t10_11 = T("Employee satisfaction surveys",   "Anonymous pulse surveys with sentiment analysis.",                p10, sp10d, devs[15],ATS.Testing,   TaskPriority.Low,      14, 8);
        var t10_12 = T("Compliance reporting – EEOC",     "Equal employment opportunity reports and audit trail.",           p10, sp10e, devs[16],ATS.ToDo,       TaskPriority.High,     8,  20);
        var t10_13 = T("Exit interview & offboarding",    "Structured exit interviews with asset return checklist.",         p10, sp10e, devs[4], ATS.ToDo,       TaskPriority.Medium,   6,  25);

        var allTasks = new[]
        {
            t1_1, t1_2, t1_3, t1_4, t1_5, t1_6, t1_7, t1_8, t1_9, t1_10, t1_11, t1_12, t1_13, t1_14,
            t2_1, t2_2, t2_3, t2_4, t2_5, t2_6, t2_7, t2_8, t2_9, t2_10, t2_11, t2_12, t2_13,
            t3_1, t3_2, t3_3, t3_4, t3_5, t3_6, t3_7, t3_8, t3_9, t3_10, t3_11, t3_12, t3_13,
            t4_1, t4_2, t4_3, t4_4, t4_5, t4_6, t4_7, t4_8, t4_9, t4_10, t4_11, t4_12, t4_13,
            t5_1, t5_2, t5_3, t5_4, t5_5, t5_6, t5_7, t5_8, t5_9, t5_10, t5_11, t5_12, t5_13,
            t6_1, t6_2, t6_3, t6_4, t6_5, t6_6, t6_7, t6_8, t6_9, t6_10, t6_11, t6_12, t6_13,
            t7_1, t7_2, t7_3, t7_4, t7_5, t7_6, t7_7, t7_8, t7_9, t7_10, t7_11, t7_12, t7_13,
            t8_1, t8_2, t8_3, t8_4, t8_5, t8_6, t8_7, t8_8, t8_9, t8_10, t8_11, t8_12, t8_13,
            t9_1, t9_2, t9_3, t9_4, t9_5, t9_6, t9_7, t9_8, t9_9, t9_10, t9_11, t9_12, t9_13,
            t10_1, t10_2, t10_3, t10_4, t10_5, t10_6, t10_7, t10_8, t10_9, t10_10, t10_11, t10_12, t10_13
        };
        db.Tasks.AddRange(allTasks);
        await db.SaveChangesAsync();

        // ── Task Dependencies ─────────────────────────────────────────
        db.TaskDependencies.AddRange(
            // Cloud Migration
            new TaskDependency { TaskId = t1_2.Id, DependsOnTaskId = t1_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t1_3.Id, DependsOnTaskId = t1_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t1_5.Id, DependsOnTaskId = t1_4.Id, TenantId = tid },
            new TaskDependency { TaskId = t1_6.Id, DependsOnTaskId = t1_5.Id, TenantId = tid },
            new TaskDependency { TaskId = t1_7.Id, DependsOnTaskId = t1_6.Id, TenantId = tid },
            new TaskDependency { TaskId = t1_8.Id, DependsOnTaskId = t1_3.Id, TenantId = tid },
            new TaskDependency { TaskId = t1_12.Id,DependsOnTaskId = t1_8.Id, TenantId = tid },
            new TaskDependency { TaskId = t1_13.Id,DependsOnTaskId = t1_7.Id, TenantId = tid },
            // Mobile App v2
            new TaskDependency { TaskId = t2_4.Id, DependsOnTaskId = t2_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t2_5.Id, DependsOnTaskId = t2_3.Id, TenantId = tid },
            new TaskDependency { TaskId = t2_8.Id, DependsOnTaskId = t2_6.Id, TenantId = tid },
            new TaskDependency { TaskId = t2_12.Id,DependsOnTaskId = t2_11.Id,TenantId = tid },
            // Customer Portal
            new TaskDependency { TaskId = t3_7.Id, DependsOnTaskId = t3_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t3_8.Id, DependsOnTaskId = t3_2.Id, TenantId = tid },
            new TaskDependency { TaskId = t3_13.Id,DependsOnTaskId = t3_6.Id, TenantId = tid },
            // E-Commerce
            new TaskDependency { TaskId = t4_4.Id, DependsOnTaskId = t4_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t4_5.Id, DependsOnTaskId = t4_3.Id, TenantId = tid },
            new TaskDependency { TaskId = t4_8.Id, DependsOnTaskId = t4_5.Id, TenantId = tid },
            new TaskDependency { TaskId = t4_13.Id,DependsOnTaskId = t4_4.Id, TenantId = tid },
            // Analytics
            new TaskDependency { TaskId = t5_3.Id, DependsOnTaskId = t5_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t5_5.Id, DependsOnTaskId = t5_4.Id, TenantId = tid },
            new TaskDependency { TaskId = t5_10.Id,DependsOnTaskId = t5_9.Id, TenantId = tid },
            // Security
            new TaskDependency { TaskId = t6_5.Id, DependsOnTaskId = t6_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t6_9.Id, DependsOnTaskId = t6_3.Id, TenantId = tid },
            new TaskDependency { TaskId = t6_13.Id,DependsOnTaskId = t6_10.Id,TenantId = tid },
            // DevOps
            new TaskDependency { TaskId = t7_3.Id, DependsOnTaskId = t7_2.Id, TenantId = tid },
            new TaskDependency { TaskId = t7_4.Id, DependsOnTaskId = t7_3.Id, TenantId = tid },
            new TaskDependency { TaskId = t7_8.Id, DependsOnTaskId = t7_5.Id, TenantId = tid },
            // AI Chatbot
            new TaskDependency { TaskId = t8_3.Id, DependsOnTaskId = t8_2.Id, TenantId = tid },
            new TaskDependency { TaskId = t8_8.Id, DependsOnTaskId = t8_7.Id, TenantId = tid },
            // Data Lake
            new TaskDependency { TaskId = t9_2.Id, DependsOnTaskId = t9_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t9_5.Id, DependsOnTaskId = t9_4.Id, TenantId = tid },
            new TaskDependency { TaskId = t9_8.Id, DependsOnTaskId = t9_3.Id, TenantId = tid },
            // HR System
            new TaskDependency { TaskId = t10_3.Id, DependsOnTaskId = t10_1.Id, TenantId = tid },
            new TaskDependency { TaskId = t10_6.Id, DependsOnTaskId = t10_4.Id, TenantId = tid },
            new TaskDependency { TaskId = t10_13.Id,DependsOnTaskId = t10_1.Id, TenantId = tid }
        );
        await db.SaveChangesAsync();

        // ── Comments (160+) ───────────────────────────────────────────
        void AddComment(TaskItem task, ApplicationUser user, string content, int daysAgo)
            => db.Comments.Add(new Comment { TaskId = task.Id, UserId = user.Id, Content = content, TenantId = tid, CreatedAt = now.AddDays(-daysAgo) });

        // Project 1 – Cloud Migration
        AddComment(t1_1, devs[0],  "VPC created with CIDR 10.0.0.0/16. Public subnets across us-east-1a, 1b, 1c.",               94);
        AddComment(t1_1, admin,    "Good work! Make sure the NAT gateway is in each AZ for HA.",                                    93);
        AddComment(t1_1, devs[1],  "Confirmed — all three NAT gateways are live. Routing tables updated.",                          92);
        AddComment(t1_2, devs[1],  "IAM roles created: EC2InstanceRole, RDSAccessRole, LambdaExecutionRole. Policies attached.",    91);
        AddComment(t1_2, devs[0],  "Reviewed the policies. We should restrict the S3 actions further — currently too broad.",        90);
        AddComment(t1_3, devs[2],  "DMS replication instance provisioned. Schema conversion tool running.",                          89);
        AddComment(t1_3, devs[0],  "DMS task shows 98.2% progress. ETA 2 hours.",                                                   88);
        AddComment(t1_3, admin,    "Excellent! Monitor the DMS CloudWatch metrics and alert on any failures.",                       87);
        AddComment(t1_4, devs[0],  "S3 buckets created with versioning. Lifecycle policies transition to Glacier after 90 days.",   79);
        AddComment(t1_5, devs[1],  "Launch template v2 created. Added user-data script for app init.",                               77);
        AddComment(t1_5, devs[4],  "ALB health checks passing on all instances. ASG policy set to CPU > 70%.",                      76);
        AddComment(t1_6, devs[2],  "CloudFront distribution deployed. Invalidation working correctly.",                              64);
        AddComment(t1_6, devs[3],  "WAF rules added for rate limiting and SQL injection protection.",                                63);
        AddComment(t1_7, devs[3],  "DNS migration plan drafted. Weighted routing: 10% traffic to new infra initially.",             62);
        AddComment(t1_8, devs[4],  "GitHub Actions workflow YAML committed. Build time is 4m 32s.",                                  59);
        AddComment(t1_9, devs[0],  "CloudWatch dashboards drafted for CPU, memory, and latency. Need alarm thresholds reviewed.",    19);
        AddComment(t1_9, admin,    "Set CPU alarm at 80%, memory at 85%, latency p99 at 500ms.",                                     18);
        AddComment(t1_10, devs[1], "RI report generated — potential 42% savings on m5.xlarge instances.",                            17);
        AddComment(t1_12, devs[4], "ECS service mesh POC complete. Envoy sidecar adds ~12ms latency. Acceptable.",                   15);

        // Project 2 – Mobile App v2
        AddComment(t2_1, devs[2],  "Expo SDK 50 scaffold complete. TypeScript strict mode enabled.",                                  91);
        AddComment(t2_2, devs[3],  "Design tokens exported from Figma. Dark mode variables ready.",                                  89);
        AddComment(t2_3, devs[4],  "FaceID prompt shows correctly on iOS. Android fingerprint tested on Pixel 7.",                   77);
        AddComment(t2_3, devs[2],  "Edge case: fallback to PIN when biometric not enrolled — handled.",                              76);
        AddComment(t2_4, devs[5],  "SQLite schema created. Sync queue with exponential backoff implemented.",                        75);
        AddComment(t2_4, devs[6],  "Conflict resolution: last-write-wins with timestamp. Should we use CRDTs?",                     74);
        AddComment(t2_4, devs[5],  "After research, CRDTs are overkill for our use case. Sticking with LWW.",                       73);
        AddComment(t2_5, devs[6],  "FCM registration complete. Deep link routing via react-navigation.",                             62);
        AddComment(t2_6, devs[4],  "iOS WidgetKit extension implemented. Android AppWidget XML layout done.",                        59);
        AddComment(t2_7, devs[3],  "Theme provider with useColorScheme hook. All 42 screens verified in dark mode.",                 57);
        AddComment(t2_8, devs[5],  "Hermes engine reducing bundle size by 35%. FPS improved from 48 to 58 on low-end devices.",     17);
        AddComment(t2_9, devs[6],  "Sentry DSN configured. First crash report captured — null pointer in navigation stack.",         15);
        AddComment(t2_10, devs[2], "RevenueCat SDK integration blocked — waiting for App Store Connect API key from Apple.",         13);
        AddComment(t2_11, devs[4], "Accessibility audit: 18 issues found, 12 fixed. Remaining 6 are low-severity contrast issues.", 11);

        // Project 3 – Customer Portal
        AddComment(t3_1, devs[0],  "Ticket system live! Auto-assignment rules configured based on category.",                        93);
        AddComment(t3_1, clients[0],"The ticket dashboard looks great. Can we add a priority filter?",                               91);
        AddComment(t3_1, devs[4],  "Priority filter added to the ticket list view.",                                                 90);
        AddComment(t3_2, devs[4],  "SLA dashboard showing breach rate at 3.2%. Heatmap looks good.",                                89);
        AddComment(t3_3, devs[5],  "Knowledge base indexed 450+ articles. Full-text search response time < 200ms.",                 77);
        AddComment(t3_4, devs[5],  "PDF generation using PuppeteerSharp. First export test: 1.2 MB, 3s render time.",                74);
        AddComment(t3_5, devs[7],  "CSAT survey email going out 24h after ticket resolution. Open rate 42%.",                        62);
        AddComment(t3_6, devs[0],  "Notification centre supports in-app toasts, email digests, and push notifications.",            59);
        AddComment(t3_7, devs[4],  "RBAC matrix drafted: 12 permissions across 4 roles. Awaiting stakeholder sign-off.",            17);
        AddComment(t3_8, devs[5],  "Ticket volume chart shows 23% increase month-over-month. Resolution time improved by 15%.",     14);
        AddComment(t3_10, devs[0], "Arabic RTL layout complete. Found 3 CSS mirror issues — fixing with logical properties.",       13);

        // Project 4 – E-Commerce Redesign
        AddComment(t4_1, devs[6],  "3-step checkout UX signed off by design team. A/B test results: +18% conversion.",               84);
        AddComment(t4_1, clients[3],"The new checkout is much smoother. Cart abandonment has dropped significantly.",                 82);
        AddComment(t4_2, devs[7],  "Recommendation model trained on 180-day purchase history. Precision@10 = 0.72.",                81);
        AddComment(t4_3, devs[8],  "Algolia instant search integrated. Results appear within 150ms.",                                71);
        AddComment(t4_4, devs[9],  "Stripe Elements v3 integrated. 3DS2 authentication tested with test cards.",                     69);
        AddComment(t4_5, devs[10], "Container Queries working on all 12 target devices. Lighthouse mobile score: 94.",              59);
        AddComment(t4_6, devs[6],  "Wishlist syncs across devices via user account. Share via link feature deployed.",               57);
        AddComment(t4_7, devs[8],  "Order tracking WebSocket pushes status updates every 30s. Map view using Mapbox.",              54);
        AddComment(t4_8, devs[9],  "LCP down from 3.8s to 2.1s after image optimisation and code splitting.",                       17);
        AddComment(t4_9, devs[10], "LaunchDarkly feature flags configured. First A/B test running on checkout CTA color.",          14);
        AddComment(t4_10, devs[7], "Added JSON-LD structured data for products. Rich snippets appearing in Google Search Console.", 13);
        AddComment(t4_11, devs[6], "Reviews module blocked — waiting for moderation service API from content team.",                 11);

        // Project 5 – Analytics Dashboard
        AddComment(t5_1, devs[11], "Burn-down chart widget complete. Velocity widget needs formula validation.",                      89);
        AddComment(t5_2, devs[12], "Linear regression R-squared = 0.89 on historical sprint data. Acceptable.",                     87);
        AddComment(t5_3, devs[13], "SignalR hub handling 200 concurrent connections. Dashboard updates in < 100ms.",                 74);
        AddComment(t5_4, devs[14], "Report builder supports line, bar, pie, scatter, and area charts. Templates saveable.",          71);
        AddComment(t5_5, devs[11], "Cron job tested — reports generated at 6 AM UTC and delivered via SendGrid.",                    59);
        AddComment(t5_6, devs[12], "Excel export uses EPPlus. Charts embedded as PNG images in the spreadsheet.",                    57);
        AddComment(t5_7, devs[13], "Theme engine supports 8 preset themes + custom CSS variables.",                                  54);
        AddComment(t5_8, devs[14], "Drill-down from sprint summary to individual task details working. Breadcrumb nav added.",       17);
        AddComment(t5_9, devs[11], "GitHub-style heatmap rendering with D3.js. Past 365 days of activity visible.",                  14);
        AddComment(t5_10, devs[12], "Researching IQR-based anomaly detection. Planning to alert on velocity drops > 2 sigma.",       11);

        // Project 6 – Security & Compliance
        AddComment(t6_1, devs[0],  "Pen test complete. 2 critical, 5 high, 12 medium findings. Remediation plan attached.",          87);
        AddComment(t6_1, admin,    "Critical findings must be resolved within 7 days. Assigning hotfix tasks.",                      86);
        AddComment(t6_2, devs[15], "PII data map complete. 23 data flows documented. Consent banners updated.",                      84);
        AddComment(t6_3, devs[16], "Nessus scans running weekly. Auto-ticketing creates Jira issues for new CVEs.",                  71);
        AddComment(t6_4, devs[16], "Splunk HEC configured. 15,000 events/min throughput. Correlation rules active.",                 69);
        AddComment(t6_5, devs[0],  "TOTP enrolment page live. Twilio SMS fallback for users without authenticator apps.",            59);
        AddComment(t6_6, devs[17], "Phishing simulation sent to 200 users. 12% click rate — need more training.",                    57);
        AddComment(t6_7, devs[15], "Redis rate limiter: 100 req/min per user, 1000 req/min per IP. 429 responses working.",          54);
        AddComment(t6_8, devs[17], "Incident response playbook draft covers: detection, containment, eradication, recovery.",        17);
        AddComment(t6_9, devs[15], "SOC 2 evidence collection: 45/62 controls documented. On track for Q3 audit.",                   15);
        AddComment(t6_10, devs[0], "AES-256 encryption implemented for all PII columns. Key rotation every 90 days.",               13);

        // Project 7 – DevOps Pipeline Automation
        AddComment(t7_1, devs[1],  "Terraform modules created for VPC, EKS, RDS, S3. State stored in S3 with DynamoDB locking.",    89);
        AddComment(t7_2, devs[3],  "EKS cluster v1.28 live. 3 node groups: general, compute-optimised, GPU.",                        87);
        AddComment(t7_3, devs[8],  "Helm chart library has 12 charts: api, worker, cron, redis, postgres, etc.",                     74);
        AddComment(t7_4, devs[14], "ArgoCD syncing 8 applications. Auto-sync with pruning enabled.",                                 71);
        AddComment(t7_5, devs[1],  "Prometheus scraping 45 targets. Grafana dashboards for node, pod, and service metrics.",         59);
        AddComment(t7_6, devs[3],  "EFK stack ingesting 2GB/day of logs. Kibana dashboards for error rate and latency.",             57);
        AddComment(t7_7, devs[8],  "Trivy scanning 23 images. Found 4 critical CVEs — all patched within 24h.",                     54);
        AddComment(t7_8, devs[14], "Blue-green deployment tested on staging. Switchover time: 12 seconds.",                          17);
        AddComment(t7_9, devs[1],  "Canary deployment: Istio VirtualService rules for traffic splitting configured.",                14);
        AddComment(t7_10, devs[3], "Vault transit engine for encryption-as-a-service. AppRole auth method for k8s pods.",            11);
        AddComment(t7_11, devs[8], "Grafana cost dashboard pulling from AWS Cost Explorer API. Monthly trend charts ready.",         13);

        // Project 8 – AI Chatbot Integration
        AddComment(t8_1, devs[5],  "Fine-tuned model on 15K support conversations. BLEU score: 0.78.",                              84);
        AddComment(t8_2, devs[9],  "Intent classifier trained on 45 intents. F1 score: 0.96 on test set.",                           81);
        AddComment(t8_3, devs[11], "Dialog flow designer supports 28 conversation paths with fallback to human agent.",              71);
        AddComment(t8_4, devs[13], "Sentiment model detecting negative sentiment with 92% accuracy. Auto-escalation working.",       69);
        AddComment(t8_5, devs[5],  "Arabic NLP model fine-tuned. Language detection accuracy: 99.2% across 3 languages.",            59);
        AddComment(t8_6, devs[9],  "Salesforce integration syncing cases bi-directionally. Average sync latency: 3.2s.",             57);
        AddComment(t8_7, devs[11], "Chat widget supports file upload (images, PDFs up to 10MB). Typing indicators smooth.",          54);
        AddComment(t8_8, devs[13], "Agent handoff preserving full conversation context. Agent sees sentiment score and intent.",      17);
        AddComment(t8_9, devs[5],  "Chatbot dashboard: 85% containment rate, 4.2/5 CSAT, avg 3.5 messages per conversation.",       14);
        AddComment(t8_10, devs[9], "FAQ auto-suggest prototype working. Top 3 relevant articles shown with confidence scores.",      11);
        AddComment(t8_11, devs[11],"Voice support blocked — waiting for Azure Speech Services enterprise contract approval.",        9);

        // Project 9 – Data Lake Architecture
        AddComment(t9_1, devs[2],  "S3 data lake zones created: raw/, cleaned/, curated/. IAM policies per zone.",                   91);
        AddComment(t9_2, devs[6],  "PySpark ETL jobs processing 2TB/day. Average job duration: 45 minutes.",                        89);
        AddComment(t9_3, devs[10], "Kafka cluster: 5 brokers, 120 partitions, 3x replication. Throughput: 100K msg/sec.",           77);
        AddComment(t9_4, devs[12], "Glue crawlers discovered 340 tables across 12 databases. Lineage graph generated.",             74);
        AddComment(t9_5, devs[2],  "Great Expectations validating 85 data quality rules. 3 expectations failing — investigating.",   62);
        AddComment(t9_6, devs[6],  "Delta Lake ACID transactions working. Time travel queries up to 30 days. Vacuum running daily.",59);
        AddComment(t9_7, devs[10], "Athena query costs reduced 68% after converting to Parquet + partitioning by date.",             57);
        AddComment(t9_8, devs[12], "Debezium CDC connector capturing 5K changes/sec from PostgreSQL to Kafka.",                     17);
        AddComment(t9_9, devs[2],  "Data classification complete: 45 PII columns tagged. Access policies enforced via Lake Formation.",14);
        AddComment(t9_10, devs[6], "Feast feature store serving 200+ features. Offline/online store sync latency < 5 min.",         13);
        AddComment(t9_11, devs[10],"Data mesh domain boundaries defined: Sales, Marketing, Product, Finance.",                       11);

        // Project 10 – HR Management System
        AddComment(t10_1, devs[4],  "Onboarding workflow: 8 steps, document upload with OCR, DocuSign e-signatures.",                87);
        AddComment(t10_2, devs[7],  "ADP integration live. Bi-directional sync running every 4 hours. Zero discrepancies so far.",  84);
        AddComment(t10_3, devs[15], "360-degree feedback form supports self, peer, manager, and skip-level reviews.",                74);
        AddComment(t10_4, devs[16], "Leave balance calculations: annual, sick, personal, parental. Accrual rules configurable.",     71);
        AddComment(t10_5, devs[4],  "Org chart renders 500+ nodes with smooth pan/zoom. Drag-drop restructuring saves instantly.",   59);
        AddComment(t10_6, devs[7],  "Self-service portal: employees can update address, emergency contacts, bank details.",          57);
        AddComment(t10_7, devs[15], "Training catalog has 85 courses. Skill gap analysis matching employees to recommended courses.",54);
        AddComment(t10_8, devs[16], "Benefits wizard: side-by-side plan comparison with cost calculator. 3 insurance providers.",     17);
        AddComment(t10_9, devs[4],  "Geofencing configured for 3 office locations. GPS accuracy within 50m radius.",                 14);
        AddComment(t10_10, devs[7], "Job posting template system ready. Interview scheduling with Calendly integration planned.",     11);
        AddComment(t10_11, devs[15],"Pulse survey module: 5 questions, anonymous responses, sentiment word cloud.",                   13);
        AddComment(t10_11, clients[3],"Can we add a question about remote work satisfaction to the survey?",                          12);
        await db.SaveChangesAsync();

        // ── Time Entries (210+) ───────────────────────────────────────
        void AddTime(TaskItem task, ApplicationUser user, int daysAgo, double startHour, double hours, string? desc = null)
        {
            var start = now.AddDays(-daysAgo).Date.AddHours(startHour);
            db.TimeEntries.Add(new TimeEntry
            {
                TaskId      = task.Id,
                UserId      = user.Id,
                TenantId    = tid,
                StartTime   = start,
                EndTime     = start.AddHours(hours),
                Description = desc ?? "Development work",
                CreatedAt   = start
            });
        }

        // Project 1 – Cloud Migration
        AddTime(t1_1, devs[0], 95, 9, 3.5, "VPC setup and subnet configuration");
        AddTime(t1_1, devs[0], 94, 9, 4,   "NAT gateway and routing tables");
        AddTime(t1_1, devs[0], 93, 14, 2,  "VPC peering and documentation");
        AddTime(t1_2, devs[1], 93, 10, 5,  "IAM roles and policy definition");
        AddTime(t1_2, devs[1], 92, 9, 3,   "IAM policy testing and refinement");
        AddTime(t1_3, devs[2], 90, 9, 6,   "DMS schema conversion");
        AddTime(t1_3, devs[2], 89, 9, 5,   "DMS replication task monitoring");
        AddTime(t1_3, devs[2], 88, 14, 3,  "Post-migration data validation");
        AddTime(t1_4, devs[0], 80, 9, 3,   "S3 bucket creation and lifecycle rules");
        AddTime(t1_4, devs[0], 79, 14, 2,  "S3 cross-region replication setup");
        AddTime(t1_5, devs[1], 78, 9, 4,   "Launch template and ASG configuration");
        AddTime(t1_5, devs[4], 77, 14, 3,  "ALB health check configuration");
        AddTime(t1_5, devs[1], 76, 9, 2.5, "ASG scaling policy tuning");
        AddTime(t1_6, devs[2], 65, 9, 4.5, "CloudFront distribution setup");
        AddTime(t1_6, devs[3], 64, 10, 2,  "WAF rules configuration");
        AddTime(t1_7, devs[3], 63, 9, 3,   "DNS migration planning");
        AddTime(t1_7, devs[3], 62, 9, 4,   "Route 53 weighted routing implementation");
        AddTime(t1_8, devs[4], 60, 10, 5,  "GitHub Actions workflow development");
        AddTime(t1_8, devs[4], 59, 9, 3,   "Docker build optimization");
        AddTime(t1_9, devs[0], 20, 9, 3,   "CloudWatch dashboard setup");
        AddTime(t1_9, devs[0], 19, 14, 2,  "Alarm configuration and SNS topics");
        AddTime(t1_10, devs[1], 18, 9, 4,  "RI analysis and Savings Plans review");
        AddTime(t1_12, devs[4], 16, 9, 5,  "ECS service mesh POC");
        AddTime(t1_12, devs[4], 15, 14, 3, "Envoy sidecar proxy configuration");

        // Project 2 – Mobile App v2
        AddTime(t2_1, devs[2], 92, 9, 4,   "Expo project scaffold");
        AddTime(t2_1, devs[2], 91, 9, 3,   "TypeScript configuration and linting");
        AddTime(t2_2, devs[3], 90, 10, 3,  "Design token implementation");
        AddTime(t2_2, devs[3], 89, 9, 2.5, "Figma token export automation");
        AddTime(t2_3, devs[4], 78, 9, 6,   "Biometric auth implementation and testing");
        AddTime(t2_3, devs[4], 77, 9, 3,   "Fallback PIN flow implementation");
        AddTime(t2_4, devs[5], 76, 9, 5,   "SQLite schema and sync engine");
        AddTime(t2_4, devs[5], 75, 9, 4,   "Conflict resolution logic");
        AddTime(t2_4, devs[5], 74, 14, 3,  "Sync queue stress testing");
        AddTime(t2_5, devs[6], 63, 9, 4,   "FCM/APNs integration");
        AddTime(t2_5, devs[6], 62, 9, 3,   "Deep link routing implementation");
        AddTime(t2_6, devs[4], 60, 9, 4.5, "Widget development iOS");
        AddTime(t2_6, devs[4], 59, 9, 3.5, "Widget development Android");
        AddTime(t2_7, devs[3], 58, 9, 4,   "Dark mode theming");
        AddTime(t2_7, devs[3], 57, 14, 3,  "Screen-by-screen dark mode verification");
        AddTime(t2_8, devs[5], 18, 9, 5,   "React Native Flipper profiling");
        AddTime(t2_8, devs[5], 17, 9, 3,   "Hermes bundle optimization");
        AddTime(t2_9, devs[6], 16, 9, 4,   "Sentry SDK integration");
        AddTime(t2_11, devs[4], 12, 9, 6,  "Accessibility audit and remediation");

        // Project 3 – Customer Portal
        AddTime(t3_1, devs[0], 94, 9, 6,   "Ticket system core implementation");
        AddTime(t3_1, devs[0], 93, 9, 4,   "Auto-assignment engine");
        AddTime(t3_1, devs[4], 91, 9, 4,   "Priority filter and UI polish");
        AddTime(t3_2, devs[4], 90, 9, 5,   "SLA dashboard components");
        AddTime(t3_2, devs[4], 89, 14, 3,  "SLA heatmap visualization");
        AddTime(t3_3, devs[5], 78, 9, 5,   "Knowledge base backend");
        AddTime(t3_3, devs[5], 77, 9, 4,   "Full-text search with Elasticsearch");
        AddTime(t3_4, devs[5], 75, 9, 4,   "PDF export pipeline");
        AddTime(t3_4, devs[5], 74, 14, 3,  "CSV export and presigned URLs");
        AddTime(t3_5, devs[7], 63, 9, 3,   "CSAT survey implementation");
        AddTime(t3_5, devs[7], 62, 14, 2.5,"Survey email automation");
        AddTime(t3_6, devs[0], 60, 9, 4,   "Notification centre backend");
        AddTime(t3_6, devs[0], 59, 9, 3,   "Email digest system");
        AddTime(t3_7, devs[4], 18, 9, 4,   "RBAC implementation");
        AddTime(t3_8, devs[5], 15, 9, 3,   "Ticket analytics charts");
        AddTime(t3_10, devs[0], 14, 9, 5,  "Arabic RTL layout implementation");

        // Project 4 – E-Commerce Redesign
        AddTime(t4_1, devs[6], 85, 9, 5,   "Checkout UX redesign");
        AddTime(t4_1, devs[6], 84, 9, 4,   "Checkout flow A/B test setup");
        AddTime(t4_2, devs[7], 82, 9, 6,   "Recommendation model training");
        AddTime(t4_2, devs[7], 81, 14, 3,  "Model evaluation and tuning");
        AddTime(t4_3, devs[8], 72, 9, 4.5, "Algolia search integration");
        AddTime(t4_3, devs[8], 71, 9, 3,   "Search facets and filters");
        AddTime(t4_4, devs[9], 70, 9, 5,   "Stripe Elements v3 integration");
        AddTime(t4_4, devs[9], 69, 9, 4,   "3DS2 and Apple/Google Pay testing");
        AddTime(t4_5, devs[10],60, 9, 4,   "Container Queries implementation");
        AddTime(t4_5, devs[10],59, 9, 3,   "Device testing (12 devices)");
        AddTime(t4_6, devs[6], 58, 9, 3.5, "Wishlist backend and sync");
        AddTime(t4_7, devs[8], 55, 9, 5,   "Order tracking WebSocket");
        AddTime(t4_7, devs[8], 54, 9, 3,   "Mapbox integration");
        AddTime(t4_8, devs[9], 18, 9, 4,   "Core Web Vitals optimization");
        AddTime(t4_8, devs[9], 17, 14, 3,  "Image optimization and lazy loading");
        AddTime(t4_9, devs[10],15, 9, 3,   "LaunchDarkly integration");
        AddTime(t4_10, devs[7], 14, 9, 4,  "SEO structured data implementation");

        // Project 5 – Analytics Dashboard
        AddTime(t5_1, devs[11],90, 9, 5,   "KPI widget development");
        AddTime(t5_1, devs[11],89, 9, 4,   "Burn-down chart algorithm");
        AddTime(t5_2, devs[12],88, 9, 6,   "Predictive analytics module");
        AddTime(t5_2, devs[12],87, 14, 3,  "Regression model validation");
        AddTime(t5_3, devs[13],75, 9, 4,   "SignalR hub implementation");
        AddTime(t5_3, devs[13],74, 9, 3,   "Real-time data push optimization");
        AddTime(t5_4, devs[14],72, 9, 5,   "Report builder UI");
        AddTime(t5_4, devs[14],71, 9, 4,   "Chart type selection and rendering");
        AddTime(t5_5, devs[11],60, 9, 4,   "Cron scheduler and report generation");
        AddTime(t5_5, devs[11],59, 14, 2,  "SendGrid email template");
        AddTime(t5_6, devs[12],58, 9, 5,   "Multi-format export engine");
        AddTime(t5_7, devs[13],55, 9, 4,   "Theme engine CSS variables");
        AddTime(t5_7, devs[13],54, 14, 3,  "Dark mode and preset themes");
        AddTime(t5_8, devs[14],18, 9, 4,   "Drill-down navigation implementation");
        AddTime(t5_9, devs[11],15, 9, 5,   "Activity heatmap with D3.js");

        // Project 6 – Security & Compliance
        AddTime(t6_1, devs[0], 88, 9, 8,   "Penetration testing coordination");
        AddTime(t6_1, devs[0], 87, 9, 4,   "Remediation plan documentation");
        AddTime(t6_2, devs[15],85, 9, 6,   "GDPR data mapping and consent forms");
        AddTime(t6_2, devs[15],84, 9, 4,   "Data flow diagrams");
        AddTime(t6_3, devs[16],72, 9, 5,   "Vulnerability scanner deployment");
        AddTime(t6_4, devs[16],70, 9, 5,   "Splunk SIEM integration");
        AddTime(t6_4, devs[16],69, 14, 3,  "Correlation rules development");
        AddTime(t6_5, devs[0], 60, 9, 4,   "2FA TOTP implementation");
        AddTime(t6_5, devs[0], 59, 9, 3,   "SMS OTP with Twilio");
        AddTime(t6_6, devs[17],58, 9, 4,   "Phishing simulation campaign");
        AddTime(t6_7, devs[15],55, 9, 5,   "Redis rate limiter implementation");
        AddTime(t6_8, devs[17],18, 9, 4,   "Incident response playbook drafting");
        AddTime(t6_9, devs[15],16, 9, 6,   "SOC 2 evidence collection");
        AddTime(t6_10, devs[0], 14, 9, 5,  "Database encryption implementation");

        // Project 7 – DevOps Pipeline Automation
        AddTime(t7_1, devs[1], 90, 9, 5,   "Terraform module development");
        AddTime(t7_1, devs[1], 89, 9, 4,   "State backend and CI integration");
        AddTime(t7_2, devs[3], 88, 9, 6,   "EKS cluster provisioning");
        AddTime(t7_2, devs[3], 87, 9, 4,   "RBAC and node group configuration");
        AddTime(t7_3, devs[8], 75, 9, 4,   "Helm chart development");
        AddTime(t7_3, devs[8], 74, 14, 3,  "Chart testing with Helm unittest");
        AddTime(t7_4, devs[14],72, 9, 5,   "ArgoCD setup and app-of-apps pattern");
        AddTime(t7_5, devs[1], 60, 9, 5,   "Prometheus + Grafana stack");
        AddTime(t7_5, devs[1], 59, 14, 3,  "Custom Grafana dashboards");
        AddTime(t7_6, devs[3], 58, 9, 4,   "EFK stack deployment");
        AddTime(t7_7, devs[8], 55, 9, 3,   "Container image scanning pipeline");
        AddTime(t7_8, devs[14],18, 9, 5,   "Blue-green deployment implementation");
        AddTime(t7_9, devs[1], 15, 9, 4,   "Istio canary deployment rules");
        AddTime(t7_11, devs[8], 14, 9, 3,  "AWS Cost Explorer API integration");

        // Project 8 – AI Chatbot Integration
        AddTime(t8_1, devs[5], 85, 9, 7,   "Model fine-tuning on support corpus");
        AddTime(t8_1, devs[5], 84, 9, 5,   "Model evaluation and iteration");
        AddTime(t8_2, devs[9], 82, 9, 6,   "Intent classifier training");
        AddTime(t8_2, devs[9], 81, 14, 3,  "Intent accuracy testing");
        AddTime(t8_3, devs[11],72, 9, 5,   "Dialog flow designer backend");
        AddTime(t8_3, devs[11],71, 9, 4,   "Dialog flow visual builder UI");
        AddTime(t8_4, devs[13],70, 9, 4,   "Sentiment analysis model");
        AddTime(t8_5, devs[5], 60, 9, 5,   "Arabic NLP model training");
        AddTime(t8_6, devs[9], 58, 9, 4,   "Salesforce REST API integration");
        AddTime(t8_7, devs[11],55, 9, 5,   "Chat widget WebSocket implementation");
        AddTime(t8_7, devs[11],54, 14, 3,  "File upload and typing indicators");
        AddTime(t8_8, devs[13],18, 9, 4,   "Agent handoff logic");
        AddTime(t8_9, devs[5], 15, 9, 5,   "Chatbot analytics dashboard");

        // Project 9 – Data Lake Architecture
        AddTime(t9_1, devs[2], 92, 9, 4,   "S3 zone architecture");
        AddTime(t9_1, devs[2], 91, 9, 3,   "IAM policies and bucket configuration");
        AddTime(t9_2, devs[6], 90, 9, 6,   "PySpark ETL job development");
        AddTime(t9_2, devs[6], 89, 9, 4,   "ETL performance optimization");
        AddTime(t9_3, devs[10],78, 9, 7,   "Kafka cluster setup and configuration");
        AddTime(t9_3, devs[10],77, 9, 4,   "Schema Registry and topic management");
        AddTime(t9_4, devs[12],75, 9, 4,   "AWS Glue crawler configuration");
        AddTime(t9_5, devs[2], 63, 9, 5,   "Great Expectations rule development");
        AddTime(t9_6, devs[6], 60, 9, 5,   "Delta Lake configuration");
        AddTime(t9_6, devs[6], 59, 14, 3,  "Time travel and vacuum setup");
        AddTime(t9_7, devs[10],58, 9, 4,   "Athena query optimization");
        AddTime(t9_8, devs[12],18, 9, 5,   "Debezium CDC connector setup");
        AddTime(t9_9, devs[2], 15, 9, 6,   "Data governance policy implementation");
        AddTime(t9_10, devs[6], 14, 9, 4,  "Feast feature store configuration");

        // Project 10 – HR Management System
        AddTime(t10_1, devs[4], 88, 9, 5,  "Onboarding workflow engine");
        AddTime(t10_1, devs[4], 87, 9, 4,  "Document upload and OCR integration");
        AddTime(t10_2, devs[7], 85, 9, 6,  "ADP API integration");
        AddTime(t10_2, devs[7], 84, 9, 4,  "Payroll sync testing");
        AddTime(t10_3, devs[15],75, 9, 5,  "Performance review forms");
        AddTime(t10_3, devs[15],74, 14, 3, "360-degree feedback logic");
        AddTime(t10_4, devs[16],72, 9, 4,  "Leave management backend");
        AddTime(t10_4, devs[16],71, 9, 3,  "Approval workflow engine");
        AddTime(t10_5, devs[4], 60, 9, 5,  "Org chart D3.js visualization");
        AddTime(t10_5, devs[4], 59, 14, 3, "Drag-drop restructuring");
        AddTime(t10_6, devs[7], 58, 9, 4,  "Self-service portal frontend");
        AddTime(t10_7, devs[15],55, 9, 4,  "Training catalog and skill matching");
        AddTime(t10_8, devs[16],18, 9, 4,  "Benefits enrollment wizard");
        AddTime(t10_9, devs[4], 15, 9, 5,  "Geofencing and clock-in system");
        AddTime(t10_11, devs[15],14, 9, 3, "Pulse survey module development");
        await db.SaveChangesAsync();

        // ── Activity Logs (85+) ───────────────────────────────────────
        void AddLog(TaskItem task, ApplicationUser actor, string action, string entity,
                    string? oldVal, string? newVal, int daysAgo)
            => db.ActivityLogs.Add(new ActivityLog
            {
                TaskId     = task.Id,
                ActorId    = actor.Id,
                Action     = action,
                EntityName = entity,
                OldValue   = oldVal,
                NewValue   = newVal,
                TenantId   = tid,
                Timestamp  = now.AddDays(-daysAgo)
            });

        // Project 1
        AddLog(t1_1, admin,   "StatusChanged",   "TaskItem", "ToDo",       "InProgress",  95);
        AddLog(t1_1, devs[0], "StatusChanged",   "TaskItem", "InProgress", "Done",         88);
        AddLog(t1_2, admin,   "Assigned",        "TaskItem", null,         devs[1].FullName, 93);
        AddLog(t1_2, devs[1], "StatusChanged",   "TaskItem", "ToDo",       "Done",         86);
        AddLog(t1_3, admin,   "PriorityChanged", "TaskItem", "Medium",     "High",         91);
        AddLog(t1_3, devs[2], "StatusChanged",   "TaskItem", "InProgress", "Done",         83);
        AddLog(t1_5, admin,   "Assigned",        "TaskItem", null,         devs[1].FullName, 78);
        AddLog(t1_5, devs[1], "StatusChanged",   "TaskItem", "ToDo",       "InProgress",   77);
        AddLog(t1_5, devs[1], "StatusChanged",   "TaskItem", "InProgress", "Done",         71);
        AddLog(t1_7, admin,   "Created",         "TaskItem", null,         t1_7.Title,     63);
        AddLog(t1_9, devs[0], "StatusChanged",   "TaskItem", "ToDo",       "InProgress",   20);
        AddLog(t1_12, devs[4], "StatusChanged",  "TaskItem", "InProgress", "InReview",     15);
        // Project 2
        AddLog(t2_3, devs[4], "StatusChanged",   "TaskItem", "InProgress", "Done",         71);
        AddLog(t2_4, devs[5], "StatusChanged",   "TaskItem", "ToDo",       "InProgress",   76);
        AddLog(t2_4, devs[5], "StatusChanged",   "TaskItem", "InProgress", "Done",         69);
        AddLog(t2_5, devs[6], "StatusChanged",   "TaskItem", "InProgress", "Done",         56);
        AddLog(t2_7, devs[3], "StatusChanged",   "TaskItem", "InProgress", "Done",         51);
        AddLog(t2_8, devs[5], "StatusChanged",   "TaskItem", "ToDo",       "InProgress",   18);
        AddLog(t2_10, admin,  "StatusChanged",   "TaskItem", "ToDo",       "Blocked",      14);
        AddLog(t2_11, devs[4], "StatusChanged",  "TaskItem", "InProgress", "InReview",     12);
        // Project 3
        AddLog(t3_1, devs[0], "StatusChanged",   "TaskItem", "InProgress", "Done",         87);
        AddLog(t3_3, devs[5], "StatusChanged",   "TaskItem", "InProgress", "Done",         71);
        AddLog(t3_4, devs[5], "StatusChanged",   "TaskItem", "InProgress", "Done",         68);
        AddLog(t3_6, devs[0], "StatusChanged",   "TaskItem", "InProgress", "Done",         53);
        AddLog(t3_7, devs[4], "StatusChanged",   "TaskItem", "ToDo",       "InProgress",   18);
        AddLog(t3_10, devs[0], "StatusChanged",  "TaskItem", "InProgress", "InReview",     14);
        // Project 4
        AddLog(t4_1, devs[6], "StatusChanged",   "TaskItem", "InProgress", "Done",         78);
        AddLog(t4_2, devs[7], "StatusChanged",   "TaskItem", "InProgress", "Done",         75);
        AddLog(t4_3, devs[8], "StatusChanged",   "TaskItem", "InProgress", "Done",         65);
        AddLog(t4_5, devs[10],"StatusChanged",   "TaskItem", "InProgress", "Done",         53);
        AddLog(t4_7, devs[8], "StatusChanged",   "TaskItem", "InProgress", "Done",         48);
        AddLog(t4_8, devs[9], "StatusChanged",   "TaskItem", "ToDo",       "InProgress",   18);
        AddLog(t4_10, devs[7], "StatusChanged",  "TaskItem", "InProgress", "InReview",     14);
        AddLog(t4_11, admin,  "StatusChanged",   "TaskItem", "ToDo",       "Blocked",      12);
        // Project 5
        AddLog(t5_1, devs[11],"StatusChanged",   "TaskItem", "InProgress", "Done",         83);
        AddLog(t5_2, devs[12],"StatusChanged",   "TaskItem", "InProgress", "Done",         81);
        AddLog(t5_3, devs[13],"StatusChanged",   "TaskItem", "InProgress", "Done",         68);
        AddLog(t5_5, devs[11],"StatusChanged",   "TaskItem", "InProgress", "Done",         53);
        AddLog(t5_7, devs[13],"StatusChanged",   "TaskItem", "InProgress", "Done",         48);
        AddLog(t5_8, devs[14],"StatusChanged",   "TaskItem", "ToDo",       "InProgress",   18);
        // Project 6
        AddLog(t6_1, devs[0], "StatusChanged",   "TaskItem", "InProgress", "Done",         81);
        AddLog(t6_2, devs[15],"StatusChanged",   "TaskItem", "InProgress", "Done",         78);
        AddLog(t6_4, devs[16],"StatusChanged",   "TaskItem", "InProgress", "Done",         63);
        AddLog(t6_5, devs[0], "StatusChanged",   "TaskItem", "InProgress", "Done",         53);
        AddLog(t6_7, devs[15],"StatusChanged",   "TaskItem", "InProgress", "Done",         48);
        AddLog(t6_8, devs[17],"StatusChanged",   "TaskItem", "ToDo",       "InProgress",   18);
        AddLog(t6_9, devs[15],"PriorityChanged", "TaskItem", "High",       "Critical",     17);
        AddLog(t6_10, devs[0], "StatusChanged",  "TaskItem", "InProgress", "InReview",     14);
        // Project 7
        AddLog(t7_1, devs[1], "StatusChanged",   "TaskItem", "InProgress", "Done",         83);
        AddLog(t7_2, devs[3], "StatusChanged",   "TaskItem", "InProgress", "Done",         81);
        AddLog(t7_4, devs[14],"StatusChanged",   "TaskItem", "InProgress", "Done",         65);
        AddLog(t7_5, devs[1], "StatusChanged",   "TaskItem", "InProgress", "Done",         53);
        AddLog(t7_7, devs[8], "StatusChanged",   "TaskItem", "InProgress", "Done",         48);
        AddLog(t7_8, devs[14],"StatusChanged",   "TaskItem", "ToDo",       "InProgress",   18);
        AddLog(t7_11, devs[8], "StatusChanged",  "TaskItem", "InProgress", "InReview",     14);
        // Project 8
        AddLog(t8_1, devs[5], "StatusChanged",   "TaskItem", "InProgress", "Done",         78);
        AddLog(t8_3, devs[11],"StatusChanged",   "TaskItem", "InProgress", "Done",         65);
        AddLog(t8_5, devs[5], "StatusChanged",   "TaskItem", "InProgress", "Done",         53);
        AddLog(t8_7, devs[11],"StatusChanged",   "TaskItem", "InProgress", "Done",         48);
        AddLog(t8_8, devs[13],"StatusChanged",   "TaskItem", "ToDo",       "InProgress",   18);
        AddLog(t8_11, admin,  "StatusChanged",   "TaskItem", "ToDo",       "Blocked",      10);
        // Project 9
        AddLog(t9_1, devs[2], "StatusChanged",   "TaskItem", "InProgress", "Done",         85);
        AddLog(t9_3, devs[10],"StatusChanged",   "TaskItem", "InProgress", "Done",         71);
        AddLog(t9_5, devs[2], "StatusChanged",   "TaskItem", "InProgress", "Done",         56);
        AddLog(t9_6, devs[6], "StatusChanged",   "TaskItem", "InProgress", "Done",         53);
        AddLog(t9_8, devs[12],"StatusChanged",   "TaskItem", "ToDo",       "InProgress",   18);
        AddLog(t9_9, admin,   "PriorityChanged", "TaskItem", "High",       "Critical",     16);
        AddLog(t9_10, devs[6], "StatusChanged",  "TaskItem", "InProgress", "InReview",     14);
        // Project 10
        AddLog(t10_1, devs[4], "StatusChanged",  "TaskItem", "InProgress", "Done",         81);
        AddLog(t10_2, devs[7], "StatusChanged",  "TaskItem", "InProgress", "Done",         78);
        AddLog(t10_4, devs[16],"StatusChanged",  "TaskItem", "InProgress", "Done",         65);
        AddLog(t10_5, devs[4], "StatusChanged",  "TaskItem", "InProgress", "Done",         53);
        AddLog(t10_7, devs[15],"StatusChanged",  "TaskItem", "InProgress", "Done",         48);
        AddLog(t10_8, devs[16],"StatusChanged",  "TaskItem", "ToDo",       "InProgress",   18);
        AddLog(t10_9, devs[4], "StatusChanged",  "TaskItem", "ToDo",       "InProgress",   15);
        AddLog(t10_11, devs[15],"StatusChanged", "TaskItem", "InProgress", "InReview",     14);
        AddLog(t10_2, admin,   "CommentAdded",   "TaskItem", null,         "ADP sync verified", 83);
        AddLog(t6_1, admin,    "AttachmentAdded","TaskItem", null,         "pentest-report-2026.pdf", 86);
        await db.SaveChangesAsync();

        // ── Notifications (65+) ───────────────────────────────────────
        void AddNotif(ApplicationUser user, string title, string msg, string type, bool read, int daysAgo)
            => db.Notifications.Add(new Notification
            {
                UserId    = user.Id,
                Title     = title,
                Message   = msg,
                Type      = type,
                IsRead    = read,
                TenantId  = tid,
                CreatedAt = now.AddDays(-daysAgo)
            });

        // Task assignments
        AddNotif(devs[0],  "Task Assigned",         "You have been assigned to 'Set up AWS VPC & subnets'",          "TaskAssigned",     true,  95);
        AddNotif(devs[1],  "Task Assigned",         "You have been assigned to 'Configure IAM roles & policies'",    "TaskAssigned",     true,  93);
        AddNotif(devs[2],  "Task Assigned",         "You have been assigned to 'Migrate PostgreSQL to RDS'",         "TaskAssigned",     true,  90);
        AddNotif(devs[5],  "Task Assigned",         "You have been assigned to 'Offline-first sync engine'",         "TaskAssigned",     true,  76);
        AddNotif(devs[6],  "Task Assigned",         "You have been assigned to 'Push notifications – FCM/APNs'",    "TaskAssigned",     true,  63);
        AddNotif(devs[8],  "Task Assigned",         "You have been assigned to 'Product listing redesign'",          "TaskAssigned",     true,  72);
        AddNotif(devs[9],  "Task Assigned",         "You have been assigned to 'Stripe payment gateway v2'",         "TaskAssigned",     true,  70);
        AddNotif(devs[13], "Task Assigned",         "You have been assigned to 'Custom report builder'",             "TaskAssigned",     true,  72);
        AddNotif(devs[16], "Task Assigned",         "You have been assigned to 'SIEM integration – Splunk'",         "TaskAssigned",     true,  70);
        AddNotif(devs[1],  "Task Assigned",         "You have been assigned to 'Terraform IaC foundation'",          "TaskAssigned",     true,  90);
        AddNotif(devs[5],  "Task Assigned",         "You have been assigned to 'NLP model selection & training'",    "TaskAssigned",     true,  85);
        AddNotif(devs[2],  "Task Assigned",         "You have been assigned to 'S3 data lake foundation'",           "TaskAssigned",     true,  92);
        AddNotif(devs[4],  "Task Assigned",         "You have been assigned to 'Employee onboarding workflow'",      "TaskAssigned",     true,  88);
        // Badge notifications
        AddNotif(devs[0],  "Badge Earned",          "Congratulations! You earned the 'Task Champion' badge 👑",       "BadgeEarned",     true,  60);
        AddNotif(devs[1],  "Badge Earned",          "Congratulations! You earned the 'Task Master' badge 🏆",         "BadgeEarned",     true,  55);
        AddNotif(devs[2],  "Badge Earned",          "Congratulations! You earned the 'Team Player' badge 🤝",         "BadgeEarned",     true,  50);
        AddNotif(devs[3],  "Badge Earned",          "Congratulations! You earned the 'Speed Runner' badge ⚡",         "BadgeEarned",     true,  45);
        AddNotif(devs[4],  "Badge Earned",          "Congratulations! You earned the 'Streak Starter' badge 🔥",      "BadgeEarned",     true,  40);
        // Comments
        AddNotif(devs[0],  "Comment on your task",  "Admin commented on 'Set up AWS VPC & subnets'",                "Comment",          true,  93);
        AddNotif(devs[4],  "Comment on your task",  "Client commented on 'Support ticket system'",                   "Comment",          true,  91);
        AddNotif(devs[6],  "Comment on your task",  "Client4 commented on 'New checkout flow UX'",                   "Comment",          true,  82);
        AddNotif(devs[5],  "Comment on your task",  "Devs discussed CRDT vs LWW on 'Offline-first sync engine'",     "Comment",          true,  74);
        // Sprint events
        AddNotif(devs[3],  "Sprint Started",        "Sprint 2 for Cloud Migration has started",                      "SprintStarted",   true,  81);
        AddNotif(devs[8],  "Sprint Started",        "Sprint 3 for E-Commerce Redesign has started",                  "SprintStarted",   true,  55);
        AddNotif(devs[1],  "Sprint Started",        "Sprint 4 for DevOps Pipeline is active",                        "SprintStarted",   false, 18);
        AddNotif(admin,    "Sprint Completed",      "Sprint 1 – Cloud Migration completed successfully",              "SprintCompleted", true,  81);
        AddNotif(admin,    "Sprint Completed",      "Sprint 2 – Mobile App v2 completed successfully",                "SprintCompleted", true,  55);
        AddNotif(admin,    "Sprint Completed",      "Sprint 3 – Data Lake Architecture completed",                    "SprintCompleted", true,  35);
        AddNotif(admin,    "Sprint Completed",      "Sprint 3 – Security & Compliance completed",                     "SprintCompleted", true,  30);
        // Budget alerts
        AddNotif(admin,    "Budget Alert",          "Cloud Migration project has consumed 35% of budget",             "BudgetAlert",     true,  60);
        AddNotif(admin,    "Budget Alert",          "E-Commerce Redesign approaching 33% budget threshold",           "BudgetAlert",     true,  50);
        AddNotif(admin,    "Budget Alert",          "Data Lake Architecture project has consumed 33% of budget",      "BudgetAlert",     false, 25);
        // XP milestones
        AddNotif(devs[0],  "XP Milestone",          "You reached Level 8! Total XP: 4800",                           "XPMilestone",     true,  35);
        AddNotif(devs[1],  "XP Milestone",          "You reached Level 7! Total XP: 3950",                           "XPMilestone",     true,  40);
        AddNotif(devs[2],  "XP Milestone",          "You reached Level 7! Total XP: 3200",                           "XPMilestone",     true,  45);
        // Deadline alerts
        AddNotif(devs[0],  "Deadline Approaching",  "'CloudWatch dashboards' deadline in 5 days",                    "DeadlineAlert",   false, 3);
        AddNotif(devs[1],  "Deadline Approaching",  "'Cost optimisation review' deadline in 8 days",                 "DeadlineAlert",   false, 2);
        AddNotif(devs[4],  "Deadline Approaching",  "'Time & attendance system' deadline in 9 days",                 "DeadlineAlert",   false, 2);
        AddNotif(devs[14], "Deadline Approaching",  "'Blue-green deployment pipeline' deadline in 6 days",           "DeadlineAlert",   false, 3);
        AddNotif(devs[12], "Deadline Approaching",  "'Real-time CDC pipeline' deadline in 6 days",                   "DeadlineAlert",   false, 3);
        AddNotif(devs[15], "Deadline Approaching",  "'SOC 2 Type II audit prep' deadline in 8 days",                 "DeadlineAlert",   false, 2);
        // Meeting notifications
        AddNotif(devs[0],  "Meeting Scheduled",     "Cloud Migration – Sprint 4 Planning scheduled for tomorrow",    "MeetingScheduled",false, 1);
        AddNotif(devs[1],  "Meeting Scheduled",     "DevOps Pipeline – Sprint 4 Review scheduled this week",         "MeetingScheduled",false, 2);
        AddNotif(devs[5],  "Meeting Scheduled",     "AI Chatbot – Client Demo scheduled for Friday",                 "MeetingScheduled",false, 1);
        AddNotif(clients[0],"Meeting Scheduled",    "Cloud Migration quarterly review meeting this Thursday",         "MeetingScheduled",false, 2);
        AddNotif(clients[1],"Meeting Scheduled",    "Mobile App – Client Demo scheduled for next week",               "MeetingScheduled",false, 3);
        // Additional task assignments for new projects
        AddNotif(devs[7],  "Task Assigned",         "You have been assigned to 'Payroll integration – ADP'",         "TaskAssigned",     true,  85);
        AddNotif(devs[3],  "Task Assigned",         "You have been assigned to 'Kubernetes cluster setup (EKS)'",    "TaskAssigned",     true,  88);
        AddNotif(devs[10], "Task Assigned",         "You have been assigned to 'Kafka streaming platform'",          "TaskAssigned",     true,  78);
        AddNotif(devs[11], "Task Assigned",         "You have been assigned to 'Conversation flow designer'",        "TaskAssigned",     false, 72);
        AddNotif(devs[12], "Task Assigned",         "You have been assigned to 'Data catalog – AWS Glue'",           "TaskAssigned",     false, 75);
        AddNotif(devs[15], "Task Assigned",         "You have been assigned to 'Performance review module'",         "TaskAssigned",     true,  75);
        AddNotif(devs[16], "Task Assigned",         "You have been assigned to 'Leave management system'",           "TaskAssigned",     true,  72);
        AddNotif(devs[14], "Task Assigned",         "You have been assigned to 'ArgoCD GitOps pipeline'",            "TaskAssigned",     true,  72);
        await db.SaveChangesAsync();

        // ── Feedbacks (22) ────────────────────────────────────────────
        db.Feedbacks.AddRange(
            new Feedback { TaskId = t3_1.Id, ClientId = clients[0].Id, Rating = 5, Comment = "The ticket system is excellent! Easy to use and very responsive.",                    TenantId = tid, CreatedAt = now.AddDays(-88) },
            new Feedback { TaskId = t3_2.Id, ClientId = clients[2].Id, Rating = 4, Comment = "SLA dashboard is informative. Would love to export it as PDF.",                       TenantId = tid, CreatedAt = now.AddDays(-86) },
            new Feedback { TaskId = t4_1.Id, ClientId = clients[3].Id, Rating = 5, Comment = "New checkout is much faster. Our conversion rate has improved!",                      TenantId = tid, CreatedAt = now.AddDays(-80) },
            new Feedback { TaskId = t1_3.Id, ClientId = clients[0].Id, Rating = 4, Comment = "Migration went smoothly with minimal downtime. Great job team!",                     TenantId = tid, CreatedAt = now.AddDays(-83) },
            new Feedback { TaskId = t5_1.Id, ClientId = clients[1].Id, Rating = 5, Comment = "The analytics dashboard is fantastic. Exactly what we needed.",                       TenantId = tid, CreatedAt = now.AddDays(-85) },
            new Feedback { TaskId = t2_3.Id, ClientId = clients[1].Id, Rating = 4, Comment = "Biometric auth works great on iOS. Android took a couple tries.",                    TenantId = tid, CreatedAt = now.AddDays(-72) },
            new Feedback { TaskId = t1_1.Id, ClientId = clients[0].Id, Rating = 5, Comment = "VPC architecture is solid. The multi-AZ setup gives us great confidence.",           TenantId = tid, CreatedAt = now.AddDays(-90) },
            new Feedback { TaskId = t4_2.Id, ClientId = clients[3].Id, Rating = 5, Comment = "Product recommendations are spot-on! Sales are up 12% since launch.",                TenantId = tid, CreatedAt = now.AddDays(-75) },
            new Feedback { TaskId = t2_5.Id, ClientId = clients[1].Id, Rating = 4, Comment = "Push notifications working well. Would like more customization options.",             TenantId = tid, CreatedAt = now.AddDays(-58) },
            new Feedback { TaskId = t3_5.Id, ClientId = clients[2].Id, Rating = 3, Comment = "CSAT survey is basic but functional. Could use more question types.",                 TenantId = tid, CreatedAt = now.AddDays(-55) },
            new Feedback { TaskId = t6_5.Id, ClientId = clients[4].Id, Rating = 5, Comment = "2FA rollout was seamless. Users adapted quickly with great documentation.",           TenantId = tid, CreatedAt = now.AddDays(-50) },
            new Feedback { TaskId = t4_5.Id, ClientId = clients[3].Id, Rating = 4, Comment = "Mobile responsive design looks great on all our test devices.",                       TenantId = tid, CreatedAt = now.AddDays(-53) },
            new Feedback { TaskId = t7_2.Id, ClientId = clients[0].Id, Rating = 5, Comment = "Kubernetes cluster is rock solid. Auto-scaling handles traffic spikes perfectly.",    TenantId = tid, CreatedAt = now.AddDays(-80) },
            new Feedback { TaskId = t8_1.Id, ClientId = clients[1].Id, Rating = 4, Comment = "AI chatbot is impressive! It handles 85% of queries without human intervention.",    TenantId = tid, CreatedAt = now.AddDays(-70) },
            new Feedback { TaskId = t9_3.Id, ClientId = clients[2].Id, Rating = 5, Comment = "Kafka streaming is handling our data volume beautifully. Zero data loss.",            TenantId = tid, CreatedAt = now.AddDays(-65) },
            new Feedback { TaskId = t10_1.Id,ClientId = clients[3].Id, Rating = 4, Comment = "Employee onboarding workflow reduced our onboarding time by 60%.",                   TenantId = tid, CreatedAt = now.AddDays(-78) },
            new Feedback { TaskId = t10_2.Id,ClientId = clients[3].Id, Rating = 3, Comment = "ADP integration works but sync delay of 4h is a bit long for payroll.",              TenantId = tid, CreatedAt = now.AddDays(-75) },
            new Feedback { TaskId = t8_7.Id, ClientId = clients[1].Id, Rating = 5, Comment = "Chat widget is beautiful and fast. Users love the typing indicators.",                TenantId = tid, CreatedAt = now.AddDays(-48) },
            new Feedback { TaskId = t5_5.Id, ClientId = clients[1].Id, Rating = 4, Comment = "Scheduled reports are very convenient. CSV format is perfect for our needs.",         TenantId = tid, CreatedAt = now.AddDays(-55) },
            new Feedback { TaskId = t7_5.Id, ClientId = clients[0].Id, Rating = 5, Comment = "Prometheus + Grafana dashboards are gorgeous. Love the alerting setup.",              TenantId = tid, CreatedAt = now.AddDays(-52) },
            new Feedback { TaskId = t3_3.Id, ClientId = clients[0].Id, Rating = 2, Comment = "Knowledge base search needs improvement — results are not always relevant.",          TenantId = tid, CreatedAt = now.AddDays(-68) },
            new Feedback { TaskId = t9_7.Id, ClientId = clients[2].Id, Rating = 4, Comment = "Athena query costs dropped dramatically after optimization. Great work!",             TenantId = tid, CreatedAt = now.AddDays(-50) }
        );
        await db.SaveChangesAsync();

        // ── Meetings (32) ─────────────────────────────────────────────
        Meeting Mtg(string title, Project p, Sprint? sp, MeetingType type, MeetingStatus status,
                    int daysAgo, double hour, double durationHours)
        {
            var sched = now.AddDays(-daysAgo).Date.AddHours(hour);
            return new Meeting
            {
                Title       = title,
                ProjectId   = p.Id,
                SprintId    = sp?.Id,
                Type        = type,
                Status      = status,
                TenantId    = tid,
                CreatedBy   = admin.Id,
                FacilitatedBy = admin.Id,
                ScheduledAt = sched,
                StartedAt   = status != MeetingStatus.Scheduled ? sched : null,
                EndedAt     = status == MeetingStatus.Completed ? sched.AddHours(durationHours) : null,
                CreatedAt   = now.AddDays(-daysAgo - 2),
                UpdatedAt   = now.AddDays(-daysAgo + 1)
            };
        }

        // Sprint planning & review meetings
        var m1  = Mtg("Cloud Migration – Sprint 1 Planning",      p1, sp1a, MeetingType.SprintPlanning,      MeetingStatus.Completed, 94, 9, 2);
        var m2  = Mtg("Mobile App v2 – Sprint 1 Planning",       p2, sp2a, MeetingType.SprintPlanning,      MeetingStatus.Completed, 91, 9, 2);
        var m3  = Mtg("Cloud Migration – Sprint 1 Review",        p1, sp1a, MeetingType.SprintReview,        MeetingStatus.Completed, 81, 14, 1.5);
        var m4  = Mtg("Cloud Migration – Sprint 1 Retrospective", p1, sp1a, MeetingType.SprintRetrospective, MeetingStatus.Completed, 80, 10, 1.5);
        var m5  = Mtg("Cloud Migration – Sprint 2 Planning",      p1, sp1b, MeetingType.SprintPlanning,      MeetingStatus.Completed, 79, 9, 2);
        var m6  = Mtg("Customer Portal – Sprint 2 Planning",      p3, sp3b, MeetingType.SprintPlanning,      MeetingStatus.Completed, 77, 9, 2);
        var m7  = Mtg("Daily Standup – Cloud Migration",          p1, sp1d, MeetingType.DailyStandup,        MeetingStatus.Completed, 5, 9.5, 0.25);
        var m8  = Mtg("Daily Standup – Mobile App v2",           p2, sp2d, MeetingType.DailyStandup,        MeetingStatus.Completed, 5, 10, 0.25);
        var m9  = Mtg("Client Meeting – Customer Portal Review",  p3, null, MeetingType.ClientMeeting,       MeetingStatus.Completed, 40, 14, 1);
        var m10 = Mtg("Security Kick-off Meeting",                p6, sp6a, MeetingType.TeamMeeting,         MeetingStatus.Completed, 87, 9, 1.5);
        var m11 = Mtg("E-Commerce Design Review",                 p4, sp4b, MeetingType.TeamMeeting,         MeetingStatus.Completed, 70, 14, 1);
        var m12 = Mtg("Analytics Sprint 2 Planning",              p5, sp5b, MeetingType.SprintPlanning,      MeetingStatus.Completed, 74, 9, 2);
        var m13 = Mtg("Cloud Migration – Sprint 4 Planning",      p1, sp1d, MeetingType.SprintPlanning,      MeetingStatus.Scheduled, 2,  9, 2);
        var m14 = Mtg("Mobile App – Client Demo",                 p2, null, MeetingType.ClientMeeting,       MeetingStatus.Scheduled, 1,  14, 1);
        // New meetings for expanded projects
        var m15 = Mtg("DevOps Pipeline – Sprint 1 Planning",      p7, sp7a, MeetingType.SprintPlanning,      MeetingStatus.Completed, 89, 9, 2);
        var m16 = Mtg("AI Chatbot – Sprint 1 Planning",           p8, sp8a, MeetingType.SprintPlanning,      MeetingStatus.Completed, 84, 9, 2);
        var m17 = Mtg("Data Lake – Sprint 1 Planning",            p9, sp9a, MeetingType.SprintPlanning,      MeetingStatus.Completed, 91, 9, 2);
        var m18 = Mtg("HR System – Sprint 1 Planning",            p10,sp10a,MeetingType.SprintPlanning,      MeetingStatus.Completed, 87, 9, 2);
        var m19 = Mtg("DevOps Pipeline – Sprint 2 Review",        p7, sp7b, MeetingType.SprintReview,        MeetingStatus.Completed, 58, 14, 1.5);
        var m20 = Mtg("AI Chatbot – Sprint 2 Retrospective",      p8, sp8b, MeetingType.SprintRetrospective, MeetingStatus.Completed, 55, 10, 1.5);
        var m21 = Mtg("Data Lake – Sprint 3 Review",              p9, sp9c, MeetingType.SprintReview,        MeetingStatus.Completed, 35, 14, 1.5);
        var m22 = Mtg("HR System – Sprint 3 Review",              p10,sp10c,MeetingType.SprintReview,        MeetingStatus.Completed, 35, 14, 1.5);
        var m23 = Mtg("Client Meeting – AI Chatbot Demo",         p8, null, MeetingType.ClientMeeting,       MeetingStatus.Completed, 30, 14, 1);
        var m24 = Mtg("Daily Standup – DevOps Pipeline",          p7, sp7d, MeetingType.DailyStandup,        MeetingStatus.Completed, 4, 9.5, 0.25);
        var m25 = Mtg("Daily Standup – AI Chatbot",               p8, sp8d, MeetingType.DailyStandup,        MeetingStatus.Completed, 4, 10, 0.25);
        var m26 = Mtg("Daily Standup – Data Lake",                p9, sp9d, MeetingType.DailyStandup,        MeetingStatus.Completed, 3, 9.5, 0.25);
        var m27 = Mtg("Daily Standup – HR System",                p10,sp10d,MeetingType.DailyStandup,        MeetingStatus.Completed, 3, 10, 0.25);
        var m28 = Mtg("Security & Compliance – Sprint 3 Retro",  p6, sp6c, MeetingType.SprintRetrospective, MeetingStatus.Completed, 30, 10, 1.5);
        var m29 = Mtg("E-Commerce – Sprint 3 Review",             p4, sp4c, MeetingType.SprintReview,        MeetingStatus.Completed, 38, 14, 1.5);
        var m30 = Mtg("Data Lake – Client Quarterly Review",      p9, null, MeetingType.ClientMeeting,       MeetingStatus.Completed, 25, 14, 1);
        var m31 = Mtg("All Hands – Architecture Review",          p1, null, MeetingType.TeamMeeting,         MeetingStatus.Scheduled, -2, 10, 2);
        var m32 = Mtg("HR System – Sprint 4 Planning",            p10,sp10d,MeetingType.SprintPlanning,      MeetingStatus.Scheduled, -1, 9, 2);

        db.Meetings.AddRange(m1,m2,m3,m4,m5,m6,m7,m8,m9,m10,m11,m12,m13,m14,
                             m15,m16,m17,m18,m19,m20,m21,m22,m23,m24,m25,m26,m27,m28,m29,m30,m31,m32);
        await db.SaveChangesAsync();

        // ── Meeting Participants ───────────────────────────────────────
        void AddParticipant(Meeting m, ApplicationUser u, ParticipantStatus pStatus, bool isFac = false)
            => db.MeetingParticipants.Add(new MeetingParticipant
            {
                MeetingId   = m.Id,
                UserId      = u.Id,
                TenantId    = tid,
                Status      = pStatus,
                IsFacilitator = isFac,
                JoinedAt    = m.StartedAt ?? m.ScheduledAt,
                LeftAt      = m.EndedAt,
                RespondedAt = m.StartedAt?.AddHours(-1)
            });

        // Sprint 1 Planning – Cloud Migration
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[1], devs[2], devs[3], devs[4] })
            AddParticipant(m1, u, ParticipantStatus.Accepted, u == admin);
        // Sprint 1 Planning – Mobile App
        foreach (var u in new ApplicationUser[] { admin, devs[2], devs[3], devs[4], devs[5], devs[6] })
            AddParticipant(m2, u, ParticipantStatus.Accepted, u == admin);
        // Sprint 1 Review – Cloud Migration
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[1], devs[2], clients[0] })
            AddParticipant(m3, u, ParticipantStatus.Accepted, u == admin);
        // Sprint 1 Retro – Cloud Migration
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[1], devs[2], devs[3], devs[4] })
            AddParticipant(m4, u, ParticipantStatus.Accepted, u == admin);
        // Sprint 2 Planning – Cloud Migration
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[1], devs[2], devs[3], devs[4] })
            AddParticipant(m5, u, ParticipantStatus.Accepted, u == admin);
        // Customer Portal Sprint 2 Planning
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[4], devs[5], devs[7] })
            AddParticipant(m6, u, ParticipantStatus.Accepted, u == admin);
        // Daily Standups
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[1], devs[2], devs[3], devs[4] })
            AddParticipant(m7, u, ParticipantStatus.Accepted, u == admin);
        foreach (var u in new ApplicationUser[] { admin, devs[2], devs[3], devs[4], devs[5] })
            AddParticipant(m8, u, ParticipantStatus.Accepted, u == admin);
        // Client Meeting
        AddParticipant(m9, admin,     ParticipantStatus.Accepted, true);
        AddParticipant(m9, devs[0],   ParticipantStatus.Accepted);
        AddParticipant(m9, clients[0],ParticipantStatus.Accepted);
        AddParticipant(m9, clients[2],ParticipantStatus.Accepted);
        // Security Kick-off
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[15], devs[16], devs[17] })
            AddParticipant(m10, u, ParticipantStatus.Accepted, u == admin);
        // E-Commerce Design Review
        foreach (var u in new ApplicationUser[] { admin, devs[6], devs[7], devs[8], devs[9], clients[3] })
            AddParticipant(m11, u, ParticipantStatus.Accepted, u == admin);
        // Analytics Sprint 2 Planning
        foreach (var u in new ApplicationUser[] { admin, devs[11], devs[12], devs[13], devs[14] })
            AddParticipant(m12, u, ParticipantStatus.Accepted, u == admin);
        // Scheduled meetings – Invited only
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[1], devs[2], devs[3], devs[4] })
            AddParticipant(m13, u, ParticipantStatus.Invited);
        AddParticipant(m14, admin,     ParticipantStatus.Invited);
        AddParticipant(m14, devs[2],   ParticipantStatus.Invited);
        AddParticipant(m14, clients[1],ParticipantStatus.Invited);
        // DevOps Sprint 1 Planning
        foreach (var u in new ApplicationUser[] { admin, devs[1], devs[3], devs[8], devs[14] })
            AddParticipant(m15, u, ParticipantStatus.Accepted, u == admin);
        // AI Chatbot Sprint 1 Planning
        foreach (var u in new ApplicationUser[] { admin, devs[5], devs[9], devs[11], devs[13] })
            AddParticipant(m16, u, ParticipantStatus.Accepted, u == admin);
        // Data Lake Sprint 1 Planning
        foreach (var u in new ApplicationUser[] { admin, devs[2], devs[6], devs[10], devs[12] })
            AddParticipant(m17, u, ParticipantStatus.Accepted, u == admin);
        // HR System Sprint 1 Planning
        foreach (var u in new ApplicationUser[] { admin, devs[4], devs[7], devs[15], devs[16] })
            AddParticipant(m18, u, ParticipantStatus.Accepted, u == admin);
        // DevOps Sprint 2 Review
        foreach (var u in new ApplicationUser[] { admin, devs[1], devs[3], devs[8], clients[0] })
            AddParticipant(m19, u, ParticipantStatus.Accepted, u == admin);
        // AI Chatbot Sprint 2 Retro
        foreach (var u in new ApplicationUser[] { admin, devs[5], devs[9], devs[11], devs[13] })
            AddParticipant(m20, u, ParticipantStatus.Accepted, u == admin);
        // Data Lake Sprint 3 Review
        foreach (var u in new ApplicationUser[] { admin, devs[2], devs[6], devs[10], clients[2] })
            AddParticipant(m21, u, ParticipantStatus.Accepted, u == admin);
        // HR System Sprint 3 Review
        foreach (var u in new ApplicationUser[] { admin, devs[4], devs[7], devs[15], clients[3] })
            AddParticipant(m22, u, ParticipantStatus.Accepted, u == admin);
        // Client Meeting – AI Chatbot Demo
        AddParticipant(m23, admin,     ParticipantStatus.Accepted, true);
        AddParticipant(m23, devs[5],   ParticipantStatus.Accepted);
        AddParticipant(m23, devs[9],   ParticipantStatus.Accepted);
        AddParticipant(m23, clients[1],ParticipantStatus.Accepted);
        // Daily Standups for new projects
        foreach (var u in new ApplicationUser[] { admin, devs[1], devs[3], devs[8], devs[14] })
            AddParticipant(m24, u, ParticipantStatus.Accepted, u == admin);
        foreach (var u in new ApplicationUser[] { admin, devs[5], devs[9], devs[11], devs[13] })
            AddParticipant(m25, u, ParticipantStatus.Accepted, u == admin);
        foreach (var u in new ApplicationUser[] { admin, devs[2], devs[6], devs[10], devs[12] })
            AddParticipant(m26, u, ParticipantStatus.Accepted, u == admin);
        foreach (var u in new ApplicationUser[] { admin, devs[4], devs[7], devs[15], devs[16] })
            AddParticipant(m27, u, ParticipantStatus.Accepted, u == admin);
        // Security Retro
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[15], devs[16], devs[17] })
            AddParticipant(m28, u, ParticipantStatus.Accepted, u == admin);
        // E-Commerce Sprint 3 Review
        foreach (var u in new ApplicationUser[] { admin, devs[6], devs[8], devs[9], devs[10], clients[3] })
            AddParticipant(m29, u, ParticipantStatus.Accepted, u == admin);
        // Data Lake Client Quarterly Review
        AddParticipant(m30, admin,     ParticipantStatus.Accepted, true);
        AddParticipant(m30, devs[2],   ParticipantStatus.Accepted);
        AddParticipant(m30, devs[6],   ParticipantStatus.Accepted);
        AddParticipant(m30, clients[2],ParticipantStatus.Accepted);
        // Scheduled – All Hands
        foreach (var u in new ApplicationUser[] { admin, devs[0], devs[1], devs[2], devs[3], devs[4], devs[5] })
            AddParticipant(m31, u, ParticipantStatus.Invited);
        // Scheduled – HR Sprint 4 Planning
        foreach (var u in new ApplicationUser[] { admin, devs[4], devs[7], devs[15], devs[16] })
            AddParticipant(m32, u, ParticipantStatus.Invited);
        await db.SaveChangesAsync();

        // ── Meeting Notes ─────────────────────────────────────────────
        void AddNote(Meeting m, ApplicationUser author, string content, NoteType type)
            => db.MeetingNotes.Add(new MeetingNote
            {
                MeetingId = m.Id,
                AuthorId  = author.Id,
                Content   = content,
                Type      = type,
                TenantId  = tid,
                IsPublic  = true,
                CreatedAt = m.EndedAt ?? m.ScheduledAt,
                UpdatedAt = m.EndedAt ?? m.ScheduledAt
            });

        AddNote(m1, admin,   "Sprint 1 Goal: Establish core AWS infrastructure including VPC, IAM, and RDS migration.", NoteType.Decision);
        AddNote(m1, admin,   "Team committed to 4 stories totalling 42 story points.", NoteType.General);
        AddNote(m1, devs[0], "VPC design reviewed and approved — 3-AZ architecture confirmed.", NoteType.Decision);
        AddNote(m3, admin,   "Sprint 1 Review: All 4 stories completed. Velocity = 42 points. Demo successful.", NoteType.General);
        AddNote(m3, admin,   "Client signed off on the VPC and RDS setup during demo.", NoteType.Decision);
        AddNote(m4, admin,   "What went well: Clear requirements, smooth VPC setup, excellent team communication.", NoteType.General);
        AddNote(m4, devs[0], "Issue: DMS job ran 6h longer than estimated. Buffer time needed for data tasks.", NoteType.Issue);
        AddNote(m4, devs[2], "Improvement: Add automated smoke tests after each deployment.", NoteType.Improvement);
        AddNote(m5, admin,   "Sprint 2 Goal: EC2 auto-scaling, CloudFront CDN, Route 53 DNS migration.", NoteType.Decision);
        AddNote(m5, devs[1], "Risk identified: Route 53 migration may require extended DNS propagation window.", NoteType.Issue);
        AddNote(m6, admin,   "Sprint 2 Goal: PDF export, CSAT surveys, and role-based access control.", NoteType.Decision);
        AddNote(m9, admin,   "Client happy with ticket system. Requested priority filter — already delivered.", NoteType.General);
        AddNote(m9, admin,   "Action: Schedule monthly review meeting with client stakeholders.", NoteType.ActionItem);
        AddNote(m10,admin,   "Kick-off: SOC 2 Type II certification target Q3 2026.", NoteType.Decision);
        AddNote(m10,devs[0], "Pen test engagement booked with external vendor for next week.", NoteType.General);
        AddNote(m11,devs[6], "New product grid approved. Algolia integration demo well received.", NoteType.General);
        AddNote(m11,admin,   "Decision: Remove legacy carousel component from product pages.", NoteType.Decision);
        AddNote(m12,admin,   "Analytics Sprint 2: Report builder and scheduled email delivery.", NoteType.Decision);
        // New meetings notes
        AddNote(m15,admin,   "DevOps Sprint 1: Terraform foundation and EKS cluster setup.", NoteType.Decision);
        AddNote(m15,devs[1], "Terraform modules will follow single-responsibility principle.", NoteType.General);
        AddNote(m16,admin,   "AI Chatbot Sprint 1: NLP model training and intent recognition.", NoteType.Decision);
        AddNote(m16,devs[5], "Will use GPT-4 fine-tuning on our 15K conversation dataset.", NoteType.General);
        AddNote(m17,admin,   "Data Lake Sprint 1: S3 foundation and Spark ETL pipelines.", NoteType.Decision);
        AddNote(m18,admin,   "HR System Sprint 1: Onboarding workflow and payroll integration.", NoteType.Decision);
        AddNote(m19,admin,   "DevOps Sprint 2: All Helm charts and ArgoCD pipelines delivered on time.", NoteType.General);
        AddNote(m20,devs[5], "Issue: Sentiment model accuracy drops for sarcastic messages.", NoteType.Issue);
        AddNote(m20,devs[9], "Improvement: Need larger training dataset for edge cases.", NoteType.Improvement);
        AddNote(m21,admin,   "Data Lake Sprint 3: Delta Lake and Athena optimization completed.", NoteType.General);
        AddNote(m22,admin,   "HR Sprint 3: Org chart and employee self-service portal delivered.", NoteType.General);
        AddNote(m23,admin,   "Client impressed with chatbot demo. 85% containment rate exceeded expectations.", NoteType.General);
        AddNote(m23,clients[1],"Would like to add voice support in the next phase.", NoteType.ActionItem);
        AddNote(m28,devs[15],"What went well: 2FA rollout had zero user complaints.", NoteType.General);
        AddNote(m28,devs[17],"What didn't go well: Phishing simulation click rate higher than expected.", NoteType.Issue);
        AddNote(m29,admin,   "E-Commerce Sprint 3: Wishlist, order tracking, and responsive overhaul done.", NoteType.General);
        AddNote(m30,admin,   "Client satisfied with data lake progress. Cost optimization requested for next quarter.", NoteType.General);
        await db.SaveChangesAsync();

        // ── Meeting Action Items ──────────────────────────────────────
        void AddActionItem(Meeting m, ApplicationUser assignee, string title, string? desc,
                           ActionItemStatus status, ActionItemPriority priority, int dueDays)
            => db.MeetingActionItems.Add(new MeetingActionItem
            {
                MeetingId    = m.Id,
                AssignedToId = assignee.Id,
                Title        = title,
                Description  = desc,
                Status       = status,
                Priority     = priority,
                DueDate      = now.AddDays(dueDays),
                TenantId     = tid,
                CreatedBy    = admin.Id,
                CreatedAt    = m.EndedAt ?? m.ScheduledAt,
                CompletedAt  = status == ActionItemStatus.Completed ? now.AddDays(-2) : null
            });

        AddActionItem(m1, devs[0], "Create VPC architecture diagram",        "Document the full VPC layout for the runbook.", ActionItemStatus.Completed, ActionItemPriority.High,   -80);
        AddActionItem(m3, admin,   "Share sprint review recording with client","Upload recording to shared drive.",             ActionItemStatus.Completed, ActionItemPriority.Medium, -78);
        AddActionItem(m4, devs[2], "Add smoke test pipeline stage",          "Add post-deploy smoke tests to CI/CD.",          ActionItemStatus.Completed, ActionItemPriority.High,   -70);
        AddActionItem(m4, devs[0], "Document DMS time estimation approach",  "Add buffer for future data migration tasks.",     ActionItemStatus.Completed, ActionItemPriority.Medium, -75);
        AddActionItem(m5, devs[3], "Draft Route 53 migration run-book",      "Step-by-step guide for DNS cutover.",            ActionItemStatus.Completed, ActionItemPriority.High,   -65);
        AddActionItem(m9, admin,   "Schedule monthly client review",         "Book recurring 1h client sync for portal.",      ActionItemStatus.Completed, ActionItemPriority.Medium, -35);
        AddActionItem(m9, devs[0], "Provide portal access to Rania Fouad",   "Create client portal account.",                  ActionItemStatus.Completed, ActionItemPriority.Low,    -38);
        AddActionItem(m10,devs[0], "Submit pen test scope to vendor",        "Include all production IP ranges.",              ActionItemStatus.Completed, ActionItemPriority.High,   -85);
        AddActionItem(m11,devs[8], "Remove legacy carousel from product pages","Clean up old component and CSS.",              ActionItemStatus.Completed, ActionItemPriority.Medium, -60);
        AddActionItem(m12,devs[14],"Set up SendGrid account for report emails","Configure SMTP credentials.",                  ActionItemStatus.Completed, ActionItemPriority.High,   -65);
        // New action items
        AddActionItem(m15,devs[1], "Create Terraform module documentation",  "README for each module with usage examples.",    ActionItemStatus.Completed, ActionItemPriority.Medium, -80);
        AddActionItem(m16,devs[5], "Prepare training dataset annotation guide","Document labeling standards for NLP training.", ActionItemStatus.Completed, ActionItemPriority.High,   -78);
        AddActionItem(m19,devs[14],"Document ArgoCD rollback procedures",    "Step-by-step guide for deployment rollback.",    ActionItemStatus.InProgress,ActionItemPriority.High,    5);
        AddActionItem(m20,devs[9], "Expand sentiment training dataset",      "Add 5K more labeled conversations.",              ActionItemStatus.InProgress,ActionItemPriority.Medium,  7);
        AddActionItem(m21,devs[2], "Fix failing data quality expectations",  "Investigate 3 failing expectations in curated zone.",ActionItemStatus.InProgress,ActionItemPriority.High, 4);
        AddActionItem(m23,devs[11],"Prototype voice channel for chatbot",    "POC using Azure Speech Services.",               ActionItemStatus.Open,      ActionItemPriority.Medium, 14);
        AddActionItem(m28,devs[17],"Conduct follow-up phishing training",    "Target users who clicked phishing link.",        ActionItemStatus.Open,      ActionItemPriority.High,   10);
        AddActionItem(m29,devs[9], "Run Lighthouse audit on all product pages","Performance audit with remediation plan.",     ActionItemStatus.InProgress,ActionItemPriority.High,    5);
        AddActionItem(m30,devs[6], "Implement S3 Intelligent-Tiering",       "Apply to curated and raw zones.",                ActionItemStatus.Open,      ActionItemPriority.Medium, 14);
        await db.SaveChangesAsync();

        // ── Standup Notes ─────────────────────────────────────────────
        var standupData = new[]
        {
            // Cloud Migration team – Days -5 to -1
            (devs[0], p1, -5, "Completed CloudFront WAF rules. Reviewed pen test findings.",
                              "Working on Route 53 migration plan. Starting EC2 ASG scaling tests.",
                              (string?)null),
            (devs[1], p1, -5, "Finished ALB health check configuration for ASG.",
                              "Continuing EC2 launch template optimisation. Code review for devs.",
                              "Waiting on security group approval from admin."),
            (devs[2], p1, -5, "Set up CloudWatch dashboard for EC2 metrics.",
                              "Adding RDS read replica to CloudWatch monitoring.",
                              (string?)null),
            (devs[0], p1, -4, "Route 53 migration runbook first draft done.",
                              "Reviewing CI/CD pipeline for blue-green deployment support.",
                              (string?)null),
            (devs[1], p1, -4, "Security group changes approved. ASG scaling policies finalized.",
                              "Testing auto-scaling under load with Locust.",
                              (string?)null),
            (devs[0], p1, -3, "CI/CD pipeline supports blue-green deployments now.",
                              "Cost optimization review — analyzing RI vs Savings Plans.",
                              (string?)null),
            (devs[0], p1, -2, "RI analysis complete. Recommending 3-year RIs for 42% savings.",
                              "Documenting cost optimization findings for stakeholder review.",
                              (string?)null),
            (devs[0], p1, -1, "Cost report submitted to admin. Waiting for budget approval.",
                              "Starting disaster recovery plan documentation.",
                              (string?)null),
            // Mobile App team
            (devs[5], p2, -5, "Sync engine conflict resolution logic complete.",
                              "Writing unit tests for offline sync scenarios.",
                              "Need clarification on merge strategy for concurrent edits."),
            (devs[6], p2, -5, "FCM deep-link routing implemented and tested.",
                              "Testing APNs with TestFlight build.",
                              (string?)null),
            (devs[5], p2, -4, "Unit tests: 45 scenarios covering all sync edge cases.",
                              "Hermes bundle optimization — targeting 30% size reduction.",
                              (string?)null),
            (devs[6], p2, -4, "APNs TestFlight build verified. Push delivery rate: 99.2%.",
                              "Starting Sentry SDK integration for crash reporting.",
                              (string?)null),
            // Analytics team
            (devs[13],p5, -4, "Report builder drag-and-drop working for line charts.",
                              "Adding bar chart and pie chart support to builder.",
                              "Third-party charting library license needs approval."),
            (devs[14],p5, -4, "SendGrid webhook configured for delivery tracking.",
                              "Wiring up cron trigger to report generation service.",
                              (string?)null),
            // Security team
            (devs[16],p6, -3, "Splunk correlation rules detecting failed login anomalies.",
                              "Setting up Splunk dashboard for security events.",
                              "Need read access to production CloudTrail logs."),
            // DevOps team
            (devs[1], p7, -3, "Canary deployment: Istio VirtualService rules configured.",
                              "Testing with 5% traffic shift. Monitoring error rates.",
                              (string?)null),
            (devs[14],p7, -3, "Blue-green deployment tested on staging. Switch time: 12s.",
                              "Adding automated rollback trigger on error rate > 5%.",
                              (string?)null),
            (devs[1], p7, -2, "Canary deployment promoted to 25%. No errors detected.",
                              "Will promote to 50% tomorrow if metrics remain stable.",
                              (string?)null),
            // AI Chatbot team
            (devs[13],p8, -3, "Agent handoff logic preserving full conversation context.",
                              "Testing with live agents for seamless transition UX.",
                              (string?)null),
            (devs[5], p8, -3, "Chatbot analytics dashboard showing 85% containment rate.",
                              "Adding top intents chart and conversation funnel view.",
                              (string?)null),
            // Data Lake team
            (devs[12],p9, -2, "Debezium CDC connector capturing 5K changes/sec.",
                              "Monitoring Kafka consumer lag. All partitions healthy.",
                              (string?)null),
            (devs[2], p9, -2, "Data governance policies: 45 PII columns tagged.",
                              "Configuring Lake Formation access controls per domain.",
                              (string?)null),
            // HR System team
            (devs[16],p10,-2, "Benefits enrollment wizard: plan comparison working.",
                              "Adding cost calculator for each insurance plan.",
                              (string?)null),
            (devs[4], p10,-2, "Geofencing configured for 3 office locations.",
                              "Testing GPS accuracy and clock-in notifications.",
                              "GPS drift causing false clock-outs near building edges."),
            (devs[4], p10,-1, "GPS drift issue resolved with 50m buffer radius.",
                              "Starting integration tests for overtime calculations.",
                              (string?)null),
        };

        foreach (var (user, proj, dayOffset, yesterday, today, blockers) in standupData)
        {
            db.StandupNotes.Add(new StandupNote
            {
                UserId                  = user.Id,
                ProjectId               = proj.Id,
                TenantId                = tid,
                Date                    = now.AddDays(dayOffset).Date,
                YesterdayAccomplishments= yesterday,
                TodayGoals              = today,
                Blockers                = blockers,
                CreatedAt               = now.AddDays(dayOffset),
                UpdatedAt               = now.AddDays(dayOffset)
            });
        }
        await db.SaveChangesAsync();

        // ── Retrospective Notes ────────────────────────────────────────
        void AddRetro(Meeting m, ApplicationUser? author, string content,
                      RetrospectiveCategory cat, int votes)
            => db.RetrospectiveNotes.Add(new RetrospectiveNote
            {
                MeetingId   = m.Id,
                AuthorId    = author?.Id,
                Content     = content,
                Category    = cat,
                Votes       = votes,
                TenantId    = tid,
                IsDiscussed = true,
                DiscussedAt = m.EndedAt,
                DiscussionSummary = "Discussed and agreed on follow-up action.",
                CreatedAt   = m.EndedAt ?? m.ScheduledAt,
                UpdatedAt   = m.EndedAt ?? m.ScheduledAt
            });

        // Cloud Migration Sprint 1 Retro (m4)
        AddRetro(m4, devs[0], "Team communication was excellent throughout the sprint.", RetrospectiveCategory.WhatWentWell, 5);
        AddRetro(m4, devs[1], "VPC and IAM setup went very smoothly with clear docs.", RetrospectiveCategory.WhatWentWell, 4);
        AddRetro(m4, devs[2], "DMS migration took 6h longer than planned — we need time buffers.", RetrospectiveCategory.WhatDidntGoWell, 6);
        AddRetro(m4, devs[3], "No staging environment — we tested directly on production clone.", RetrospectiveCategory.WhatDidntGoWell, 3);
        AddRetro(m4, devs[4], "Add automated smoke tests to CI/CD pipeline after each deploy.", RetrospectiveCategory.Improvements, 7);
        AddRetro(m4, devs[0], "Create time buffer estimates for data-heavy migration tasks.", RetrospectiveCategory.Improvements, 5);
        AddRetro(m4, admin,   "Shout-out to Diaa for the exceptional IAM policy review!", RetrospectiveCategory.Appreciations, 8);
        AddRetro(m4, devs[2], "Thanks to Ahmed for late-night DMS monitoring support.", RetrospectiveCategory.Appreciations, 6);
        // AI Chatbot Sprint 2 Retro (m20)
        AddRetro(m20, devs[5], "NLP model accuracy exceeded target — 96% intent recognition.", RetrospectiveCategory.WhatWentWell, 6);
        AddRetro(m20, devs[9], "CRM integration with Salesforce completed ahead of schedule.", RetrospectiveCategory.WhatWentWell, 5);
        AddRetro(m20, devs[11],"Sentiment model struggles with sarcastic messages.", RetrospectiveCategory.WhatDidntGoWell, 4);
        AddRetro(m20, devs[13],"Need more diverse training data for edge cases.", RetrospectiveCategory.Improvements, 5);
        AddRetro(m20, admin,   "Great work by Hassan on the multi-language NLP model!", RetrospectiveCategory.Appreciations, 7);
        // Security Sprint 3 Retro (m28)
        AddRetro(m28, devs[15],"2FA rollout was seamless. Zero complaints from users.", RetrospectiveCategory.WhatWentWell, 7);
        AddRetro(m28, devs[0], "Rate limiter caught 3 brute-force attempts in first week.", RetrospectiveCategory.WhatWentWell, 5);
        AddRetro(m28, devs[17],"Phishing click rate of 12% is concerning. Need more training.", RetrospectiveCategory.WhatDidntGoWell, 6);
        AddRetro(m28, devs[16],"Vulnerability scans should run daily, not weekly.", RetrospectiveCategory.Improvements, 4);
        AddRetro(m28, admin,   "Thanks to Karim for the excellent Splunk correlation rules!", RetrospectiveCategory.Appreciations, 6);
        await db.SaveChangesAsync();

        // ── XP Histories (45+) ────────────────────────────────────────
        var xpEvents = new[]
        {
            (devs[0], 100, "Task Completed", "Task: Set up AWS VPC & subnets",         8, 88,  false),
            (devs[0], 100, "Task Completed", "Task: Set up S3 buckets & lifecycle",     8, 73,  false),
            (devs[0], 100, "Task Completed", "Task: Penetration testing",               8, 81,  false),
            (devs[0], 100, "Task Completed", "Task: 2FA rollout – TOTP + SMS",          8, 53,  false),
            (devs[0], 200, "Badge Earned",   "Badge: Task Master",                      7, 60,  true),
            (devs[0], 500, "Badge Earned",   "Badge: Task Champion",                    8, 55,  false),
            (devs[0], 250, "Badge Earned",   "Badge: On Fire (7-day streak)",           8, 50,  false),
            (devs[1], 100, "Task Completed", "Task: Configure IAM roles & policies",    7, 86,  false),
            (devs[1], 100, "Task Completed", "Task: Deploy EC2 auto-scaling group",     7, 71,  false),
            (devs[1], 100, "Task Completed", "Task: Terraform IaC foundation",          7, 83,  false),
            (devs[1], 100, "Task Completed", "Task: Prometheus monitoring stack",        7, 53,  false),
            (devs[1], 200, "Badge Earned",   "Badge: Task Master",                      7, 55,  true),
            (devs[1], 500, "Badge Earned",   "Badge: Task Champion",                    7, 50,  false),
            (devs[2], 100, "Task Completed", "Task: Migrate PostgreSQL to RDS",         7, 83,  false),
            (devs[2], 100, "Task Completed", "Task: S3 data lake foundation",           7, 85,  false),
            (devs[2], 100, "Task Completed", "Task: Data quality framework",             7, 56,  false),
            (devs[2], 200, "Badge Earned",   "Badge: Task Master",                      7, 50,  false),
            (devs[3], 100, "Task Completed", "Task: Design system tokens",              6, 83,  false),
            (devs[3], 100, "Task Completed", "Task: Kubernetes cluster setup",           6, 81,  false),
            (devs[3], 75,  "Badge Earned",   "Badge: Speed Runner",                     6, 75,  false),
            (devs[4], 100, "Task Completed", "Task: Biometric authentication",          6, 71,  false),
            (devs[4], 100, "Task Completed", "Task: Employee onboarding workflow",       6, 81,  false),
            (devs[4], 100, "Task Completed", "Task: Org chart visualization",            6, 53,  false),
            (devs[5], 100, "Task Completed", "Task: Offline-first sync engine",          5, 69,  false),
            (devs[5], 100, "Task Completed", "Task: NLP model selection & training",     5, 78,  false),
            (devs[5], 50,  "Comment Posted", "Active discussion on CRDT strategy",       5, 73,  false),
            (devs[6], 100, "Task Completed", "Task: Push notifications – FCM/APNs",     5, 56,  false),
            (devs[6], 100, "Task Completed", "Task: Delta Lake implementation",           5, 53,  false),
            (devs[7], 100, "Task Completed", "Task: Recommendation engine",              5, 75,  false),
            (devs[7], 100, "Task Completed", "Task: Payroll integration – ADP",          5, 78,  false),
            (devs[8], 100, "Task Completed", "Task: Product listing redesign",            5, 65,  false),
            (devs[8], 100, "Task Completed", "Task: Helm chart library",                  5, 68,  false),
            (devs[9], 100, "Task Completed", "Task: Stripe payment gateway v2",           5, 63,  false),
            (devs[9], 100, "Task Completed", "Task: Intent recognition pipeline",         5, 75,  false),
            (devs[10],100, "Task Completed", "Task: Mobile responsive overhaul",          4, 53,  false),
            (devs[10],100, "Task Completed", "Task: Kafka streaming platform",            4, 71,  false),
            (devs[11],100, "Task Completed", "Task: KPI tracking widgets",                4, 83,  false),
            (devs[11],100, "Task Completed", "Task: Conversation flow designer",          4, 65,  false),
            (devs[12],100, "Task Completed", "Task: Predictive analytics module",         4, 81,  false),
            (devs[12],100, "Task Completed", "Task: Data catalog – AWS Glue",             4, 68,  false),
            (devs[13],50,  "Task Completed", "Task: Real-time data pipeline",             3, 68,  false),
            (devs[14],50,  "Task Completed", "Task: Custom report builder",               3, 65,  false),
            (devs[14],50,  "Task Completed", "Task: GitOps with ArgoCD",                  3, 65,  false),
            (devs[15],50,  "Task Completed", "Task: GDPR data mapping",                   3, 78,  false),
            (devs[16],50,  "Task Completed", "Task: SIEM integration – Splunk",           2, 63,  false),
        };

        foreach (var (user, xp, reason, src, level, daysAgo, wasLevelUp) in xpEvents)
        {
            db.XPHistories.Add(new XPHistory
            {
                UserId      = user.Id,
                TenantId    = tid,
                XPAmount    = xp,
                Reason      = reason,
                SourceId    = src,
                LevelAtTime = level,
                WasLevelUp  = wasLevelUp,
                AwardedAt   = now.AddDays(-daysAgo)
            });
        }
        await db.SaveChangesAsync();

        // ── Attachments (18) ──────────────────────────────────────────
        var attachments = new[]
        {
            (t1_1, devs[0],  "vpc-architecture-diagram.png",      "/uploads/vpc-architecture-diagram.png",        "image/png",          245760L, 90),
            (t1_3, devs[2],  "dms-migration-report.pdf",          "/uploads/dms-migration-report.pdf",            "application/pdf",    512000L, 85),
            (t1_8, devs[4],  "github-actions-workflow.yml",       "/uploads/github-actions-workflow.yml",         "text/yaml",           12800L, 58),
            (t2_2, devs[3],  "design-tokens-figma-export.json",   "/uploads/design-tokens-figma-export.json",     "application/json",    45056L, 88),
            (t2_11, devs[4], "accessibility-audit-report.xlsx",   "/uploads/accessibility-audit-report.xlsx",     "application/xlsx",   189440L, 11),
            (t3_2, devs[4],  "sla-dashboard-screenshot.png",      "/uploads/sla-dashboard-screenshot.png",        "image/png",          348160L, 88),
            (t4_2, devs[7],  "recommendation-model-metrics.csv",  "/uploads/recommendation-model-metrics.csv",    "text/csv",            28672L, 80),
            (t4_8, devs[9],  "lighthouse-report-before.html",     "/uploads/lighthouse-report-before.html",       "text/html",          102400L, 17),
            (t5_2, devs[12], "regression-analysis-results.pdf",   "/uploads/regression-analysis-results.pdf",     "application/pdf",    256000L, 86),
            (t6_1, devs[0],  "pentest-report-2026.pdf",           "/uploads/pentest-report-2026.pdf",             "application/pdf",    896000L, 86),
            (t6_2, devs[15], "gdpr-data-flow-diagram.svg",        "/uploads/gdpr-data-flow-diagram.svg",          "image/svg+xml",       78000L, 83),
            (t7_1, devs[1],  "terraform-module-diagram.png",      "/uploads/terraform-module-diagram.png",        "image/png",          312000L, 88),
            (t7_2, devs[3],  "eks-cluster-architecture.pdf",      "/uploads/eks-cluster-architecture.pdf",        "application/pdf",    420000L, 86),
            (t8_1, devs[5],  "nlp-model-training-metrics.csv",    "/uploads/nlp-model-training-metrics.csv",      "text/csv",            56000L, 83),
            (t8_4, devs[13], "sentiment-confusion-matrix.png",    "/uploads/sentiment-confusion-matrix.png",      "image/png",          185000L, 68),
            (t9_3, devs[10], "kafka-cluster-topology.svg",        "/uploads/kafka-cluster-topology.svg",          "image/svg+xml",       92000L, 76),
            (t10_1, devs[4], "onboarding-workflow-diagram.pdf",   "/uploads/onboarding-workflow-diagram.pdf",     "application/pdf",    340000L, 86),
            (t10_5, devs[4], "org-chart-screenshot.png",          "/uploads/org-chart-screenshot.png",            "image/png",          420000L, 58),
        };

        foreach (var (task, user, fileName, filePath, fileType, fileSize, daysAgo) in attachments)
        {
            db.Attachments.Add(new Attachment
            {
                TaskId          = task.Id,
                FileName        = fileName,
                FilePath        = filePath,
                FileSizeBytes   = fileSize,
                UploadedByUserId= user.Id,
                TenantId        = tid,
                UploadedAt      = now.AddDays(-daysAgo)
            });
        }
        await db.SaveChangesAsync();

        // ── Time Reports (8) ──────────────────────────────────────────
        db.TimeReports.AddRange(
            new TimeReport
            {
                Title       = "Cloud Migration – Weekly Report (Week 18)",
                Description = "Weekly time tracking summary for Cloud Migration project.",
                TenantId    = tid,
                GeneratedBy = admin.Id,
                StartDate   = now.AddDays(-14),
                EndDate     = now.AddDays(-7),
                Type        = ReportType.Weekly,
                Scope       = ReportScope.Project,
                ProjectId   = p1.Id,
                TotalHours  = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30)),
                TotalAmount = 8700,
                ReportData  = "{\"totalTasks\":5,\"completedTasks\":3,\"topContributor\":\"Diaa Naguib\"}",
                IsPublic    = false,
                GeneratedAt = now.AddDays(-7)
            },
            new TimeReport
            {
                Title       = "All Projects – Monthly Report (April 2026)",
                Description = "Consolidated monthly time and billing report.",
                TenantId    = tid,
                GeneratedBy = admin.Id,
                StartDate   = now.AddDays(-60),
                EndDate     = now.AddDays(-30),
                Type        = ReportType.Monthly,
                Scope       = ReportScope.All,
                TotalHours  = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(20)),
                TotalAmount = 124000,
                ReportData  = "{\"totalProjects\":10,\"totalDevelopers\":18,\"avgVelocity\":42}",
                IsPublic    = true,
                GeneratedAt = now.AddDays(-30)
            },
            new TimeReport
            {
                Title       = "Diaa Naguib – Weekly Time Report",
                Description = "Individual time tracking report for Diaa Naguib.",
                TenantId    = tid,
                GeneratedBy = devs[0].Id,
                UserId      = devs[0].Id,
                StartDate   = now.AddDays(-7),
                EndDate     = now,
                Type        = ReportType.Weekly,
                Scope       = ReportScope.User,
                TotalHours  = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(42)),
                TotalAmount = 3850,
                ReportData  = "{\"tasks\":[\"CloudWatch dashboards\",\"Cost optimization\",\"Disaster recovery\"],\"totalHours\":38.5}",
                IsPublic    = false,
                GeneratedAt = now.AddDays(-1)
            },
            new TimeReport
            {
                Title       = "DevOps Pipeline – Monthly Report (April 2026)",
                Description = "Monthly time tracking summary for DevOps Pipeline project.",
                TenantId    = tid,
                GeneratedBy = admin.Id,
                StartDate   = now.AddDays(-60),
                EndDate     = now.AddDays(-30),
                Type        = ReportType.Monthly,
                Scope       = ReportScope.Project,
                ProjectId   = p7.Id,
                TotalHours  = TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(45)),
                TotalAmount = 72000,
                ReportData  = "{\"totalTasks\":8,\"completedTasks\":7,\"topContributor\":\"Ahmed Hassan\"}",
                IsPublic    = false,
                GeneratedAt = now.AddDays(-30)
            },
            new TimeReport
            {
                Title       = "AI Chatbot – Custom Report (Q1 2026)",
                Description = "Quarterly custom report for AI Chatbot project.",
                TenantId    = tid,
                GeneratedBy = admin.Id,
                StartDate   = new DateTime(2026,1,1),
                EndDate     = new DateTime(2026,3,31),
                Type        = ReportType.Custom,
                Scope       = ReportScope.Project,
                ProjectId   = p8.Id,
                TotalHours  = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)),
                TotalAmount = 86000,
                ReportData  = "{\"totalTasks\":7,\"completedTasks\":7,\"containmentRate\":0.85}",
                IsPublic    = true,
                GeneratedAt = now.AddDays(-45)
            },
            new TimeReport
            {
                Title       = "Engineering Team – Weekly Report",
                Description = "Team-level weekly time report for all engineering members.",
                TenantId    = tid,
                GeneratedBy = admin.Id,
                StartDate   = now.AddDays(-7),
                EndDate     = now,
                Type        = ReportType.Weekly,
                Scope       = ReportScope.Team,
                TotalHours  = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(10)),
                TotalAmount = 42000,
                ReportData  = "{\"teamSize\":18,\"avgHoursPerDev\":37.8,\"utilizationRate\":0.94}",
                IsPublic    = false,
                GeneratedAt = now.AddDays(-1)
            },
            new TimeReport
            {
                Title       = "Data Lake Architecture – Monthly Report (April 2026)",
                Description = "Monthly time and cost report for Data Lake project.",
                TenantId    = tid,
                GeneratedBy = admin.Id,
                StartDate   = now.AddDays(-60),
                EndDate     = now.AddDays(-30),
                Type        = ReportType.Monthly,
                Scope       = ReportScope.Project,
                ProjectId   = p9.Id,
                TotalHours  = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30)),
                TotalAmount = 93000,
                ReportData  = "{\"totalTasks\":9,\"completedTasks\":7,\"dataProcessed\":\"2TB/day\"}",
                IsPublic    = false,
                GeneratedAt = now.AddDays(-30)
            },
            new TimeReport
            {
                Title       = "Ahmed Hassan – Monthly Time Report",
                Description = "Individual monthly time tracking report for Ahmed Hassan.",
                TenantId    = tid,
                GeneratedBy = devs[1].Id,
                UserId      = devs[1].Id,
                StartDate   = now.AddDays(-30),
                EndDate     = now,
                Type        = ReportType.Monthly,
                Scope       = ReportScope.User,
                TotalHours  = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(5)),
                TotalAmount = 8400,
                ReportData  = "{\"tasks\":[\"Terraform\",\"Prometheus\",\"Canary deployments\"],\"totalHours\":161}",
                IsPublic    = false,
                GeneratedAt = now.AddDays(-1)
            }
        );
        await db.SaveChangesAsync();

        // ── Report Templates (6) ──────────────────────────────────────
        var rt1 = new ReportTemplate
        {
            TenantId          = tid,
            Name              = "Sprint Velocity Report",
            Description       = "Tracks sprint velocity over time with trend analysis.",
            ReportType        = "SprintVelocity",
            DefaultFormat     = "PDF",
            ScheduleFrequency = "Weekly",
            CreatedBy         = admin.Id,
            Parameters        = "{\"includeBurndown\":true,\"includeVelocityChart\":true}",
            IsActive          = true,
            CreatedAt         = now.AddDays(-90),
            UpdatedAt         = now.AddDays(-30)
        };
        var rt2 = new ReportTemplate
        {
            TenantId          = tid,
            Name              = "Team Performance Report",
            Description       = "Individual and team KPIs: tasks completed, XP, time logged.",
            ReportType        = "TeamPerformance",
            DefaultFormat     = "Excel",
            ScheduleFrequency = "Monthly",
            CreatedBy         = admin.Id,
            Parameters        = "{\"includeXP\":true,\"includeBadges\":true,\"includeTimeEntries\":true}",
            IsActive          = true,
            CreatedAt         = now.AddDays(-80),
            UpdatedAt         = now.AddDays(-20)
        };
        var rt3 = new ReportTemplate
        {
            TenantId          = tid,
            Name              = "Budget Utilisation Report",
            Description       = "Project budget vs actual cost with forecast to project end.",
            ReportType        = "BudgetUtilisation",
            DefaultFormat     = "PDF",
            ScheduleFrequency = "Monthly",
            CreatedBy         = admin.Id,
            Parameters        = "{\"includeForecast\":true,\"currency\":\"USD\"}",
            IsActive          = true,
            CreatedAt         = now.AddDays(-75),
            UpdatedAt         = now.AddDays(-15)
        };
        var rt4 = new ReportTemplate
        {
            TenantId          = tid,
            Name              = "Security Compliance Report",
            Description       = "SOC 2 control evidence, vulnerability scan results, and incident summary.",
            ReportType        = "SecurityCompliance",
            DefaultFormat     = "PDF",
            ScheduleFrequency = "Monthly",
            CreatedBy         = admin.Id,
            Parameters        = "{\"includeVulnScans\":true,\"includeIncidents\":true,\"includeSOC2Progress\":true}",
            IsActive          = true,
            CreatedAt         = now.AddDays(-60),
            UpdatedAt         = now.AddDays(-10)
        };
        var rt5 = new ReportTemplate
        {
            TenantId          = tid,
            Name              = "Resource Utilisation Report",
            Description       = "Developer allocation, billable hours, and capacity planning.",
            ReportType        = "ResourceUtilisation",
            DefaultFormat     = "Excel",
            ScheduleFrequency = "Weekly",
            CreatedBy         = admin.Id,
            Parameters        = "{\"includeCapacityForecast\":true,\"includeAllocation\":true}",
            IsActive          = true,
            CreatedAt         = now.AddDays(-55),
            UpdatedAt         = now.AddDays(-5)
        };
        var rt6 = new ReportTemplate
        {
            TenantId          = tid,
            Name              = "Client Satisfaction Report",
            Description       = "Aggregated client feedback, CSAT scores, and trend analysis.",
            ReportType        = "ClientSatisfaction",
            DefaultFormat     = "PDF",
            ScheduleFrequency = "Monthly",
            CreatedBy         = admin.Id,
            Parameters        = "{\"includeCSAT\":true,\"includeNPS\":true,\"includeTrends\":true}",
            IsActive          = true,
            CreatedAt         = now.AddDays(-50),
            UpdatedAt         = now.AddDays(-3)
        };
        db.ReportTemplates.AddRange(rt1, rt2, rt3, rt4, rt5, rt6);
        await db.SaveChangesAsync();

        // ── Scheduled Reports (5) ─────────────────────────────────────
        db.ScheduledReports.AddRange(
            new ScheduledReport
            {
                TenantId          = tid,
                TemplateId        = rt1.Id,
                ScheduleFrequency = "Weekly",
                NextRun           = now.AddDays(7),
                LastRun           = now.AddDays(-7),
                IsActive          = true,
                Recipients        = "[\"Tadrous@gmail.com\",\"Diaa@gmail.com\"]",
                Parameters        = "{\"projectId\":1}",
                CreatedAt         = now.AddDays(-60),
                UpdatedAt         = now.AddDays(-7)
            },
            new ScheduledReport
            {
                TenantId          = tid,
                TemplateId        = rt2.Id,
                ScheduleFrequency = "Monthly",
                NextRun           = now.AddDays(10),
                LastRun           = now.AddDays(-20),
                IsActive          = true,
                Recipients        = "[\"Tadrous@gmail.com\"]",
                Parameters        = "{\"includeAllTeams\":true}",
                CreatedAt         = now.AddDays(-50),
                UpdatedAt         = now.AddDays(-20)
            },
            new ScheduledReport
            {
                TenantId          = tid,
                TemplateId        = rt3.Id,
                ScheduleFrequency = "Monthly",
                NextRun           = now.AddDays(15),
                LastRun           = now.AddDays(-15),
                IsActive          = true,
                Recipients        = "[\"Tadrous@gmail.com\",\"Saad@gmail.com\"]",
                Parameters        = "{\"allProjects\":true,\"currency\":\"USD\"}",
                CreatedAt         = now.AddDays(-45),
                UpdatedAt         = now.AddDays(-15)
            },
            new ScheduledReport
            {
                TenantId          = tid,
                TemplateId        = rt4.Id,
                ScheduleFrequency = "Monthly",
                NextRun           = now.AddDays(12),
                LastRun           = now.AddDays(-18),
                IsActive          = true,
                Recipients        = "[\"Tadrous@gmail.com\",\"Diaa@gmail.com\"]",
                Parameters        = "{\"includeSOC2\":true}",
                CreatedAt         = now.AddDays(-40),
                UpdatedAt         = now.AddDays(-18)
            },
            new ScheduledReport
            {
                TenantId          = tid,
                TemplateId        = rt5.Id,
                ScheduleFrequency = "Weekly",
                NextRun           = now.AddDays(5),
                LastRun           = now.AddDays(-2),
                IsActive          = true,
                Recipients        = "[\"Tadrous@gmail.com\"]",
                Parameters        = "{\"includeAllDevelopers\":true}",
                CreatedAt         = now.AddDays(-30),
                UpdatedAt         = now.AddDays(-2)
            }
        );
        await db.SaveChangesAsync();

        // ── Report Histories (8) ──────────────────────────────────────
        db.ReportHistories.AddRange(
            new ReportHistory
            {
                TenantId    = tid,
                ReportId    = "RPT-2026-001",
                ReportName  = "Sprint Velocity Report – Week 18",
                ReportType  = "SprintVelocity",
                GeneratedBy = admin.Id,
                GeneratedAt = now.AddDays(-7),
                FileSize    = 245760,
                Format      = "PDF",
                Status      = "Success",
                DownloadUrl = "/reports/RPT-2026-001.pdf",
                ExpiresAt   = now.AddDays(23)
            },
            new ReportHistory
            {
                TenantId    = tid,
                ReportId    = "RPT-2026-002",
                ReportName  = "Team Performance – April 2026",
                ReportType  = "TeamPerformance",
                GeneratedBy = admin.Id,
                GeneratedAt = now.AddDays(-30),
                FileSize    = 512000,
                Format      = "Excel",
                Status      = "Success",
                DownloadUrl = "/reports/RPT-2026-002.xlsx",
                ExpiresAt   = now.AddDays(0)
            },
            new ReportHistory
            {
                TenantId    = tid,
                ReportId    = "RPT-2026-003",
                ReportName  = "Budget Utilisation – Q1 2026",
                ReportType  = "BudgetUtilisation",
                GeneratedBy = admin.Id,
                GeneratedAt = now.AddDays(-60),
                FileSize    = 189440,
                Format      = "PDF",
                Status      = "Success",
                DownloadUrl = "/reports/RPT-2026-003.pdf",
                ExpiresAt   = now.AddDays(-30)
            },
            new ReportHistory
            {
                TenantId    = tid,
                ReportId    = "RPT-2026-004",
                ReportName  = "Security Compliance – March 2026",
                ReportType  = "SecurityCompliance",
                GeneratedBy = admin.Id,
                GeneratedAt = now.AddDays(-45),
                FileSize    = 680000,
                Format      = "PDF",
                Status      = "Success",
                DownloadUrl = "/reports/RPT-2026-004.pdf",
                ExpiresAt   = now.AddDays(-15)
            },
            new ReportHistory
            {
                TenantId    = tid,
                ReportId    = "RPT-2026-005",
                ReportName  = "Resource Utilisation – Week 19",
                ReportType  = "ResourceUtilisation",
                GeneratedBy = admin.Id,
                GeneratedAt = now.AddDays(-2),
                FileSize    = 320000,
                Format      = "Excel",
                Status      = "Success",
                DownloadUrl = "/reports/RPT-2026-005.xlsx",
                ExpiresAt   = now.AddDays(28)
            },
            new ReportHistory
            {
                TenantId    = tid,
                ReportId    = "RPT-2026-006",
                ReportName  = "Client Satisfaction – April 2026",
                ReportType  = "ClientSatisfaction",
                GeneratedBy = admin.Id,
                GeneratedAt = now.AddDays(-20),
                FileSize    = 156000,
                Format      = "PDF",
                Status      = "Success",
                DownloadUrl = "/reports/RPT-2026-006.pdf",
                ExpiresAt   = now.AddDays(10)
            },
            new ReportHistory
            {
                TenantId    = tid,
                ReportId    = "RPT-2026-007",
                ReportName  = "Sprint Velocity Report – Week 17",
                ReportType  = "SprintVelocity",
                GeneratedBy = admin.Id,
                GeneratedAt = now.AddDays(-14),
                FileSize    = 238000,
                Format      = "PDF",
                Status      = "Success",
                DownloadUrl = "/reports/RPT-2026-007.pdf",
                ExpiresAt   = now.AddDays(16)
            },
            new ReportHistory
            {
                TenantId    = tid,
                ReportId    = "RPT-2026-008",
                ReportName  = "DevOps Pipeline – Quarterly Report",
                ReportType  = "Custom",
                GeneratedBy = admin.Id,
                GeneratedAt = now.AddDays(-10),
                FileSize    = 0,
                Format      = "PDF",
                Status      = "Failed",
                ErrorMessage= "Report generation timed out after 120 seconds. Data volume too large.",
                DownloadUrl = null,
                ExpiresAt   = null
            }
        );
        await db.SaveChangesAsync();

        // ── Custom Report Fields (10) ──────────────────────────────────
        db.CustomReportFields.AddRange(
            new CustomReportField { TenantId = tid, ReportType = "SprintVelocity",       Name = "include_burndown",    Label = "Include Burn-down Chart",   Type = "boolean", Required = false, DefaultValue = "true",  Order = 1, IsActive = true },
            new CustomReportField { TenantId = tid, ReportType = "SprintVelocity",       Name = "sprint_count",        Label = "Number of Sprints",          Type = "number",  Required = true,  DefaultValue = "6",     Order = 2, IsActive = true },
            new CustomReportField { TenantId = tid, ReportType = "TeamPerformance",      Name = "include_xp",          Label = "Include XP Data",            Type = "boolean", Required = false, DefaultValue = "true",  Order = 1, IsActive = true },
            new CustomReportField { TenantId = tid, ReportType = "TeamPerformance",      Name = "date_range",          Label = "Date Range (days)",          Type = "number",  Required = true,  DefaultValue = "30",    Order = 2, IsActive = true },
            new CustomReportField { TenantId = tid, ReportType = "BudgetUtilisation",    Name = "include_forecast",    Label = "Include Forecast",           Type = "boolean", Required = false, DefaultValue = "true",  Order = 1, IsActive = true },
            new CustomReportField { TenantId = tid, ReportType = "BudgetUtilisation",    Name = "currency",            Label = "Currency",                   Type = "select",  Required = true,  DefaultValue = "USD",   Order = 2, IsActive = true, Options = "[\"USD\",\"EUR\",\"GBP\",\"EGP\"]" },
            new CustomReportField { TenantId = tid, ReportType = "SecurityCompliance",   Name = "include_vuln_scans",  Label = "Include Vulnerability Scans",Type = "boolean", Required = false, DefaultValue = "true",  Order = 1, IsActive = true },
            new CustomReportField { TenantId = tid, ReportType = "SecurityCompliance",   Name = "compliance_framework",Label = "Compliance Framework",       Type = "select",  Required = true,  DefaultValue = "SOC2",  Order = 2, IsActive = true, Options = "[\"SOC2\",\"ISO27001\",\"GDPR\",\"HIPAA\"]" },
            new CustomReportField { TenantId = tid, ReportType = "ResourceUtilisation",  Name = "include_capacity",    Label = "Include Capacity Forecast",  Type = "boolean", Required = false, DefaultValue = "true",  Order = 1, IsActive = true },
            new CustomReportField { TenantId = tid, ReportType = "ClientSatisfaction",   Name = "include_nps",         Label = "Include NPS Score",          Type = "boolean", Required = false, DefaultValue = "true",  Order = 1, IsActive = true }
        );
        await db.SaveChangesAsync();

        // ── Report Permissions (16) ────────────────────────────────────
        db.ReportPermissions.AddRange(
            new ReportPermission { TenantId = tid, UserId = admin.Id,     ReportType = "SprintVelocity",      Permission = "View,Generate,Schedule", GrantedAt = now.AddDays(-90), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = admin.Id,     ReportType = "TeamPerformance",     Permission = "View,Generate,Schedule", GrantedAt = now.AddDays(-90), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = admin.Id,     ReportType = "BudgetUtilisation",   Permission = "View,Generate,Schedule", GrantedAt = now.AddDays(-90), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = admin.Id,     ReportType = "SecurityCompliance",  Permission = "View,Generate,Schedule", GrantedAt = now.AddDays(-60), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = admin.Id,     ReportType = "ResourceUtilisation", Permission = "View,Generate,Schedule", GrantedAt = now.AddDays(-55), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = admin.Id,     ReportType = "ClientSatisfaction",  Permission = "View,Generate,Schedule", GrantedAt = now.AddDays(-50), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = devs[0].Id,   ReportType = "SprintVelocity",      Permission = "View,Generate",          GrantedAt = now.AddDays(-60), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = devs[0].Id,   ReportType = "TeamPerformance",     Permission = "View",                   GrantedAt = now.AddDays(-60), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = devs[0].Id,   ReportType = "SecurityCompliance",  Permission = "View,Generate",          GrantedAt = now.AddDays(-55), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = devs[1].Id,   ReportType = "SprintVelocity",      Permission = "View",                   GrantedAt = now.AddDays(-45), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = devs[1].Id,   ReportType = "ResourceUtilisation", Permission = "View",                   GrantedAt = now.AddDays(-40), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = devs[2].Id,   ReportType = "SprintVelocity",      Permission = "View",                   GrantedAt = now.AddDays(-40), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = clients[0].Id,ReportType = "BudgetUtilisation",   Permission = "View",                   GrantedAt = now.AddDays(-30), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = clients[1].Id,ReportType = "ClientSatisfaction",  Permission = "View",                   GrantedAt = now.AddDays(-25), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = clients[2].Id,ReportType = "BudgetUtilisation",   Permission = "View",                   GrantedAt = now.AddDays(-25), GrantedBy = admin.Id },
            new ReportPermission { TenantId = tid, UserId = clients[3].Id,ReportType = "ClientSatisfaction",  Permission = "View",                   GrantedAt = now.AddDays(-20), GrantedBy = admin.Id }
        );
        await db.SaveChangesAsync();

        // ── User Security Logs (32) ───────────────────────────────────
        var securityEvents = new[]
        {
            (admin,     "Login",          "Successful login from web browser",          "192.168.1.100", "Mozilla/5.0 Chrome/124",    -1),
            (admin,     "Login",          "Successful login from web browser",          "192.168.1.100", "Mozilla/5.0 Chrome/124",    -2),
            (admin,     "Login",          "Successful login from web browser",          "192.168.1.100", "Mozilla/5.0 Chrome/124",    -3),
            (devs[0],   "Login",          "Successful login from web browser",          "10.0.1.15",     "Mozilla/5.0 Chrome/124",    -1),
            (devs[0],   "Login",          "Successful login from mobile app",           "10.0.1.15",     "AgileTaskManager iOS/2.1",  -3),
            (devs[0],   "Login",          "Successful login from web browser",          "10.0.1.15",     "Mozilla/5.0 Chrome/124",    -5),
            (devs[1],   "Login",          "Successful login from web browser",          "10.0.1.22",     "Mozilla/5.0 Firefox/125",   -1),
            (devs[1],   "Login",          "Successful login from web browser",          "10.0.1.22",     "Mozilla/5.0 Firefox/125",   -4),
            (devs[2],   "Login",          "Successful login",                           "10.0.1.30",     "Mozilla/5.0 Safari/17",     -2),
            (devs[3],   "Login",          "Successful login from web browser",          "10.0.1.35",     "Mozilla/5.0 Chrome/124",    -1),
            (devs[4],   "Login",          "Successful login from web browser",          "10.0.1.40",     "Mozilla/5.0 Chrome/124",    -2),
            (devs[5],   "Login",          "Successful login from web browser",          "10.0.1.42",     "Mozilla/5.0 Firefox/125",   -1),
            (devs[6],   "Login",          "Successful login from web browser",          "10.0.1.45",     "Mozilla/5.0 Chrome/124",    -3),
            (devs[9],   "Login",          "Successful login from web browser",          "10.0.1.55",     "Mozilla/5.0 Chrome/124",    -2),
            (devs[11],  "Login",          "Successful login from web browser",          "10.0.1.60",     "Mozilla/5.0 Safari/17",     -1),
            (devs[15],  "Login",          "Successful login from web browser",          "10.0.1.70",     "Mozilla/5.0 Chrome/124",    -1),
            (clients[0],"Login",          "Client portal login",                        "203.0.113.45",  "Mozilla/5.0 Chrome/124",    -1),
            (clients[1],"Login",          "Client portal login",                        "203.0.113.55",  "Mozilla/5.0 Safari/17",     -2),
            (clients[3],"Login",          "Client portal login",                        "203.0.113.88",  "Mozilla/5.0 Edge/124",      -3),
            (admin,     "PasswordChange", "Password changed successfully",              "192.168.1.100", "Mozilla/5.0 Chrome/124",    -30),
            (devs[0],   "PasswordChange", "Password changed successfully",              "10.0.1.15",     "Mozilla/5.0 Chrome/124",    -25),
            (devs[16],  "LoginFailed",    "Failed login attempt – wrong password",      "198.51.100.5",  "Unknown",                   -5),
            (devs[16],  "LoginFailed",    "Failed login attempt – wrong password",      "198.51.100.5",  "Unknown",                   -5),
            (devs[16],  "Login",          "Successful login after failed attempts",     "10.0.1.45",     "Mozilla/5.0 Chrome/124",    -5),
            (admin,     "RoleChanged",    "Role assigned: Developer to TeamLead",        "192.168.1.100", "Mozilla/5.0 Chrome/124",    -85),
            (devs[0],   "2FAEnabled",     "Two-factor authentication enabled",          "10.0.1.15",     "Mozilla/5.0 Chrome/124",    -50),
            (admin,     "2FAEnabled",     "Two-factor authentication enabled",          "192.168.1.100", "Mozilla/5.0 Chrome/124",    -48),
            (devs[1],   "2FAEnabled",     "Two-factor authentication enabled",          "10.0.1.22",     "Mozilla/5.0 Firefox/125",   -45),
            (devs[2],   "2FAEnabled",     "Two-factor authentication enabled",          "10.0.1.30",     "Mozilla/5.0 Safari/17",     -44),
            (devs[5],   "2FAEnabled",     "Two-factor authentication enabled",          "10.0.1.42",     "Mozilla/5.0 Firefox/125",   -40),
            (admin,     "APIKeyCreated",  "New API key generated for CI/CD integration","192.168.1.100", "Mozilla/5.0 Chrome/124",    -20),
            (devs[0],   "SessionExpired", "Session expired due to inactivity",          "10.0.1.15",     "Mozilla/5.0 Chrome/124",    -8),
        };

        foreach (var (user, evtType, desc, ip, ua, daysAgo) in securityEvents)
        {
            db.UserSecurityLogs.Add(new UserSecurityLog
            {
                TenantId       = tid,
                UserId         = user.Id,
                EventType      = evtType,
                Description    = desc,
                IpAddress      = ip,
                UserAgent      = ua,
                Timestamp      = now.AddDays(daysAgo)
            });
        }
        await db.SaveChangesAsync();
    }

    // ── Wipe ──────────────────────────────────────────────────────────
    private static async Task WipeAllDataAsync(ApplicationDbContext context)
    {
        if (!await context.Database.CanConnectAsync()) return;
        try
        {
            // Disable foreign key constraints temporarily
            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Tenants ON; ALTER TABLE Tenants NOCHECK CONSTRAINT ALL;");
            
            var sql = new[]
            {
                "DELETE FROM MeetingParticipants","DELETE FROM MeetingActionItems",
                "DELETE FROM RetrospectiveNotes","DELETE FROM MeetingNotes",
                "DELETE FROM StandupNotes","DELETE FROM Meetings",
                "DELETE FROM XPHistories","DELETE FROM UserBadges",
                "DELETE FROM Badges","DELETE FROM UserProfiles",
                "DELETE FROM Notifications","DELETE FROM Feedbacks",
                "DELETE FROM ActivityLogs","DELETE FROM Comments",
                "DELETE FROM TimeEntries","DELETE FROM TaskDependencies",
                "DELETE FROM Attachments",
                "DELETE FROM Tasks","DELETE FROM ProjectMembers",
                "DELETE FROM Sprints","DELETE FROM Projects",
                "DELETE FROM ReportPermissions","DELETE FROM CustomReportFields",
                "DELETE FROM ScheduledReports","DELETE FROM ReportHistories",
                "DELETE FROM ReportTemplates","DELETE FROM TimeReports",
                "DELETE FROM UserSecurityLogs",
                "DELETE FROM AspNetUserRoles","DELETE FROM AspNetUserClaims",
                "DELETE FROM AspNetUserLogins","DELETE FROM AspNetUserTokens",
                "DELETE FROM AspNetUsers","DELETE FROM AspNetRoles",
                "DELETE FROM Tenants",
            };
            foreach (var s in sql)
                try { await context.Database.ExecuteSqlRawAsync(s); } catch { }
            
            // Reset identity seeds for all tables
            var resetIdentity = new[]
            {
                "DBCC CHECKIDENT ('Tenants', RESEED, 0)",
                "DBCC CHECKIDENT ('Projects', RESEED, 0)",
                "DBCC CHECKIDENT ('Sprints', RESEED, 0)",
                "DBCC CHECKIDENT ('Tasks', RESEED, 0)",
                "DBCC CHECKIDENT ('Badges', RESEED, 0)",
                "DBCC CHECKIDENT ('UserProfiles', RESEED, 0)",
            };
            foreach (var s in resetIdentity)
                try { await context.Database.ExecuteSqlRawAsync(s); } catch { }
            
            // Re-enable foreign key constraints
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE Tenants WITH CHECK CHECK CONSTRAINT ALL; SET IDENTITY_INSERT Tenants OFF;");
        }
        catch { }
    }
}
