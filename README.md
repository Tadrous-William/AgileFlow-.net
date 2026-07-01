# Agile Task Manager

An enterprise-grade, multi-tenant agile project management system built with **ASP.NET Core 9 MVC** and **.NET 9**. Designed for software development teams, it covers the full agile workflow — from task and sprint management through to meetings, time tracking, analytics, gamification, and real-time collaboration.

---

## ✨ Key Features

| Feature | What it does |
|---|---|
| 📋 **Task Management** | Full CRUD with priorities, statuses, dependencies, comments, attachments, activity logs, and client feedback |
| 🗂️ **Kanban Board** | Drag-and-drop board with real-time card movement via SignalR |
| 🏃 **Sprint Management** | Planned → Active → Completed lifecycle; linked to projects, tasks, and meetings |
| 📁 **Project Management** | Budget tracking, per-project member roles, and start/end date planning |
| ⏱️ **Time Tracking** | Start/stop timers per task, timesheets with approval workflow (Draft → Submitted → Approved/Rejected), billable hours, hourly rates |
| 📅 **Meeting Management** | 8 meeting types (Standups, Planning, Retrospectives, etc.), participants, typed notes, action items, retrospective voting |
| 📊 **Analytics & Reporting** | Sprint velocity, team performance charts via Chart.js; export to Excel (EPPlus) and PDF (QuestPDF) |
| 🎮 **Gamification** | XP points, levelling, badges, streaks, team leaderboard, and real-time achievement toast notifications |
| 🔔 **Notifications** | Real-time in-app notifications via SignalR per user |
| 🤖 **AI Assistant** | OpenAI integration to generate task descriptions, suggest titles and tags (with rule-based fallback) |
| 🔐 **Security** | 2FA, account lock/unlock, suspicious login detection, session revocation, security audit log |
| 📱 **PWA Support** | Service Worker, offline fallback, background sync, push notifications, installable on mobile/desktop |
| 📬 **Email** | Transactional emails via MailKit (SMTP/Gmail) |
| 🗃️ **Auto Seed & Repair** | Database seeds 35+ demo users, 3 projects, 7 sprints, 20 tasks, meetings, badges, and time entries on first run. Includes migration repair tooling. |

---

## 🛠️ Tech Stack

### Backend
- **ASP.NET Core 9 MVC** — server-rendered Razor views + REST API
- **Entity Framework Core 9** with SQL Server / LocalDB
- **ASP.NET Core Identity** — authentication, role management, lockout, 2FA
- **SignalR** — real-time bi-directional communication (2 hubs)
- **AutoMapper 12** — entity ↔ ViewModel mapping
- **MailKit 4** — SMTP email delivery
- **EPPlus 5** — Excel export
- **QuestPDF** — PDF report generation
- **CloudinaryDotNet** — cloud file/image storage
- **Swashbuckle (Swagger) 6.5** — REST API documentation

### Frontend
- **Razor Views (.cshtml)** — server-side templating
- **Vanilla CSS** — custom design system (`site.css`, `mobile.css`, `achievement-toast.css`)
- **Chart.js** — analytics charts and dashboards
- **Vanilla JavaScript** — 9 dedicated modules (~200 KB total):
  - `kanban.js` — drag-and-drop Kanban board
  - `meeting.js` — meeting management UI
  - `time-tracking.js` — timer and timesheet UI
  - `analytics.js` — Chart.js visualisations
  - `ai-assistant.js` — AI assistant panel
  - `dependency.js` — task dependency graph
  - `mobile.js` — mobile-specific interactions
  - `achievement-toast.js` — gamification toast pop-ups
  - `site.js` — global utilities

### Progressive Web App
- **Service Worker (`sw.js`)** — static/dynamic caching, offline support, push notifications, background sync
- **`manifest.json`** — installable PWA with 9 icon sizes, 3 app shortcuts, screenshots

---

## 👥 User Roles

The application supports **4 active roles**, each with a tailored dashboard and permissions:

| Role | Dashboard | Access |
|---|---|---|
| **Admin** | System overview, all users/projects, security console, reports | Full control over everything |
| **TeamLead** | Project KPIs, sprint velocity, team leaderboard, meetings summary | Manages projects, sprints, tasks, and team members |
| **Developer** | My tasks, XP/level, active timer, badges, standup notes | Works on tasks, logs time, participates in meetings |
| **Client** | Project status, milestone progress, sprint overview, task completion | Read-only view of project progress; can leave feedback |

---

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server or SQL Server LocalDB (included with Visual Studio)
- Visual Studio 2022, VS Code, or JetBrains Rider

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd AgileTaskManager
```

### 2. Configure the Database

Open `appsettings.json` and update the connection string if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=AgileTaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

For a full SQL Server instance, replace `(localdb)\\MSSQLLocalDB` with your server name, e.g.:
```
Server=localhost;Database=AgileTaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;
```

### 3. Restore EF Tools (one-time per machine)

```bash
dotnet tool restore
```

### 4. Run the App

```bash
dotnet run
```

The app will **automatically apply migrations and seed all demo data** on first run. Navigate to:

```
https://localhost:5001
```

> **Note:** You do not need to run `dotnet ef database update` manually. The app handles migrations on startup.

---

## 🔑 Demo Credentials

All demo accounts share the same password:

> **Password for all accounts: `Tad123456!`**

### Key Accounts

| Role | Email | Description |
|---|---|---|
| **Admin** | `Tadrous@gmail.com` | Full system administrator |
| **Developer** | `Diaa@gmail.com` | Primary developer (highest XP) |
| **Developer** | `Ahmed@gmail.com` | Developer — Cloud Migration project |
| **Client** | `Saad@gmail.com` | Client with access to Cloud Migration & Customer Portal |

### Additional Developer Accounts

The seeder creates 30 developer accounts following the pattern `<Name>@gmail.com`:

`Mohamed`, `Omar`, `Ali`, `Hassan`, `Youssef`, `Kareem`, `Ibrahim`, `Mahmoud`, `Tariq`, `Nabil`, `Samir`, `Walid`, `Fadi`, `Rami`, `Karim`, `Hani`, `Jamil`, `Bassem`, `Sameh`, `Adel`, `Wael`, `Mazen`, `Rashid`, `Farid`, `Nader`, `Sami`, `Amir`

### Additional Client Accounts

`Client1@gmail.com` through `Client5@gmail.com`

---

## ⚙️ Configuration

All settings live in `appsettings.json`. Use **User Secrets** in development to avoid committing sensitive keys:

```bash
dotnet user-secrets set "AISettings:ApiKey" "sk-..."
```

### Available Settings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Agile Task Manager",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "AdminSettings": {
    "RegistrationSecret": "5544"
  },
  "AISettings": {
    "Provider": "OpenAI",
    "ApiKey": "",
    "Model": "gpt-3.5-turbo",
    "Endpoint": "https://api.openai.com/v1/chat/completions",
    "MaxTokens": 500,
    "Temperature": 0.7
  }
}
```

| Setting | Required | Notes |
|---|---|---|
| `DefaultConnection` | ✅ Yes | SQL Server connection string |
| `EmailSettings` | ⚠️ Optional | Leave blank to disable email sending |
| `CloudinarySettings` | ⚠️ Optional | Leave blank to use local file storage |
| `AISettings:ApiKey` | ⚠️ Optional | Leave blank to use built-in rule-based fallback |
| `AdminSettings:RegistrationSecret` | ✅ Yes | Required for Admin account registration |

---

## 🌐 REST API

The app exposes a full REST API under `/api/`. Swagger UI is available at `/swagger` when running in development.

| Controller | Base Route | Key Operations |
|---|---|---|
| Tasks | `/api/tasks` | CRUD, status patch |
| Sprints | `/api/sprints` | CRUD, status transitions |
| Projects | `/api/projects` | CRUD, member management |
| Kanban | `/api/kanban` | Board data, drag-and-drop moves |
| Time Tracking | `/api/timetracking` | Start/stop timers, timesheets, reports |
| Meetings | `/api/meetings` | Meetings, participants, notes, retrospectives |
| Analytics | `/api/analytics` | Velocity, team stats, completion rates |
| Reporting | `/api/reporting` | Report templates, generation, history |
| Task Dependencies | `/api/taskdependencies` | Dependency graph CRUD |
| AI Assistant | `/api/aiassistant` | Description/title/tag generation |
| Users | `/api/users` | User lookups |

All API endpoints require authentication. Role-based access is enforced per route.

---

## 🏗️ Architecture

### Design Patterns

- **MVC + Service Layer** — all business logic is isolated in service classes behind interfaces, injected via DI
- **Multi-Tenancy** — every entity implements `ITenantEntity`. `TenantMiddleware` resolves the current tenant per request and scopes all DB queries automatically
- **Policy-Based Authorization** — three system-level policies (`Admin`, `DeveloperOrAbove`, `ClientOrAbove`) plus a custom `ProjectMemberRequirementHandler` for project-level access control
- **AutoMapper** — strict separation between EF entities and ViewModels; all mappings declared in `MappingProfile.cs`
- **Global Exception Handling** — `ErrorHandlingMiddleware` (pipeline) + `GlobalExceptionFilter` (MVC action filter)
- **Domain Events** — `ITaskCompletedEvent` and `ICommentAddedEvent` for decoupled side effects
- **In-Memory Cache** — `CacheService` with expiry, priority levels, and pattern-based invalidation

### SignalR Hubs

| Hub | Route | Purpose |
|---|---|---|
| `TaskHub` | `/taskHub` | Task comment broadcast, status updates, Kanban drag-and-drop sync, per-user notifications |
| `AchievementHub` | `/achievementHub` | Real-time badge/level-up toast delivery |

---

## 📦 NuGet Packages

| Package | Version |
|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 9.0.4 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.4 |
| `Microsoft.EntityFrameworkCore.Tools` | 9.0.4 |
| `Microsoft.EntityFrameworkCore.Design` | 9.0.4 |
| `MailKit` | 4.16.0 |
| `EPPlus` | 5.8.14 |
| `CloudinaryDotNet` | 1.26.0 |
| `AutoMapper.Extensions.Microsoft.DependencyInjection` | 12.0.1 |
| `Swashbuckle.AspNetCore` | 6.5.0 |
| `QuestPDF` | 2023.12.6 |

---

## 📁 Project Structure

```
AgileTaskManager/
├── Controllers/
│   ├── AIAssistantController.cs
│   ├── AnalyticsController.cs
│   ├── DependencyController.cs
│   ├── KanbanController.cs
│   ├── MeetingController.cs
│   ├── OtherControllers.cs          # Dashboard, Account, User, Notification, Feedback
│   ├── SprintController.cs
│   ├── TaskController.cs
│   ├── TimeTrackingController.cs
│   └── Api/                         # 11 REST API controllers
│       ├── TasksApiController.cs
│       ├── SprintsApiController.cs
│       ├── ProjectsApiController.cs
│       ├── KanbanApiController.cs
│       ├── TimeTrackingApiController.cs
│       ├── MeetingApiController.cs
│       ├── AnalyticsApiController.cs
│       ├── ReportingApiController.cs
│       ├── TaskDependencyApiController.cs
│       ├── AIAssistantApiController.cs
│       └── UsersApiController.cs
│
├── Models/
│   ├── Entities/                    # All EF Core entity classes
│   │   ├── ApplicationUser.cs       # Extends IdentityUser; XP, profile, tenancy
│   │   ├── TaskItem.cs              # Tasks + dependency graph
│   │   ├── OtherEntities.cs         # Project, Sprint, Comment, Notification, ActivityLog, Attachment, Feedback
│   │   ├── MeetingEntities.cs       # Meeting, Participant, Note, ActionItem, Standup, Retrospective
│   │   ├── TimeTrackingEntities.cs  # TimeEntry, TimeSheet, TimeReport, TimeTrackingSettings
│   │   ├── GamificationEntities.cs  # UserProfile, Badge, UserBadge, XPHistory, TeamLeaderboard
│   │   ├── Tenant.cs
│   │   └── ...                      # Reporting entities, SecurityLog, UserStats
│   ├── Enums/                       # TaskPriority, TaskStatus, SprintStatus, ProjectMemberRole, UserRole
│   ├── Interfaces/                  # ITenantEntity
│   └── ViewModels/                  # 15 ViewModel files — one per feature area
│
├── Data/
│   ├── ApplicationDbContext.cs      # EF DbContext with all DbSets
│   ├── DatabaseMigrationRepair.cs   # Auto-detects and repairs broken migration history
│   └── DbSeeder.cs                  # Seeds full demo dataset on every startup
│
├── Services/
│   ├── Interfaces/                  # Interface definitions for all services
│   ├── Services.cs                  # Comment, Notification, Audit, Email, Export, Dashboard, User services
│   ├── TaskService.cs
│   ├── SprintService.cs
│   ├── MeetingService.cs
│   ├── TimeTrackingService.cs
│   ├── TaskDependencyService.cs
│   ├── ReportingService.cs
│   ├── SecurityService.cs
│   ├── AIAssistantService.cs
│   ├── CacheService.cs
│   ├── TenantContext.cs
│   └── MappingProfile.cs            # AutoMapper configuration
│
├── Hubs/
│   ├── TaskHub.cs                   # Real-time tasks, Kanban, notifications
│   └── AchievementHub.cs            # Gamification badge toasts
│
├── Authorization/
│   ├── ProjectMemberRequirement.cs
│   └── ProjectMemberRequirementHandler.cs
│
├── Middleware/
│   ├── TenantMiddleware.cs          # Resolves tenant per request
│   └── ErrorHandlingMiddleware.cs
│
├── Filters/
│   └── GlobalExceptionFilter.cs
│
├── Events/
│   ├── TaskCompletedEvent.cs
│   └── CommentAddedEvent.cs
│
├── Constants/
│   └── RoleConstants.cs             # Admin, TeamLead, Developer, Client, Viewer
│
├── Views/
│   ├── Shared/                      # _Layout.cshtml, partial views
│   ├── Dashboard/                   # Admin, TeamLead, Developer, Client dashboards
│   ├── Task/                        # Index, Details, Create, Edit
│   ├── Sprint/
│   ├── Kanban/
│   ├── Meeting/
│   ├── TimeTracking/
│   ├── Analytics/
│   ├── Dependency/
│   ├── AIAssistant/
│   ├── Account/                     # Login, Register, AccessDenied
│   ├── User/
│   ├── Notification/
│   └── Feedback/
│
├── wwwroot/
│   ├── css/
│   │   ├── site.css                 # Main design system
│   │   ├── mobile.css               # Responsive/mobile styles
│   │   └── achievement-toast.css    # Gamification toast animations
│   ├── js/
│   │   ├── kanban.js
│   │   ├── meeting.js
│   │   ├── time-tracking.js
│   │   ├── analytics.js
│   │   ├── ai-assistant.js
│   │   ├── dependency.js
│   │   ├── mobile.js
│   │   ├── achievement-toast.js
│   │   └── site.js
│   ├── manifest.json                # PWA manifest
│   └── sw.js                        # Service Worker
│
├── Migrations/                      # EF Core migration history
├── Program.cs                       # App bootstrap, DI, middleware pipeline
├── appsettings.json                 # Configuration
└── AgileTaskManager.csproj
```

---

## 🔄 Database Behaviour on Startup

Every time the app starts:

1. **Migration repair** runs — detects broken schema states and fixes them automatically
2. **Pending migrations** are applied
3. **Seeder wipes all data** and re-seeds fresh demo data (projects, users, tasks, meetings, badges, time entries, notifications, etc.)

> ⚠️ **This means data is reset on every restart.** This behaviour is intentional for demo/development purposes. To disable the wipe-and-reseed, remove the `await WipeAllDataAsync(context)` call in `DbSeeder.cs`.

---

## 🎮 Gamification System

The built-in gamification system keeps developers engaged:

- **XP Points** — earned for completing tasks, posting comments, maintaining daily streaks
- **Levels** — 6 levels calculated from cumulative XP (Level 1 → Level 6)
- **Badges** — 5 badge types seeded by default (First Task, Task Master, Commenter, Streak, Bug Hunter), each with an XP reward and JSON-based criteria
- **Leaderboard** — per-project ranking by XP
- **Real-time toasts** — when a badge is earned, an animated toast appears instantly via `AchievementHub` (SignalR)

---

## 📱 PWA (Progressive Web App)

The app is fully installable as a PWA:

- Accessible from the browser's **"Install App"** prompt on Chrome/Edge/Safari
- Works **offline** — static assets and recent API responses are cached by the Service Worker
- Supports **push notifications** and **background sync** when returning online
- App shortcuts: **Create Task**, **View Dashboard**, **Scan QR Code**

---

## 🔐 Security Features

- Password policy: minimum 8 characters, uppercase, lowercase, digit, and special character required
- Account lockout after 5 failed login attempts within 1 hour
- **Two-Factor Authentication (2FA)** — enable/disable per user
- **Security audit log** — every login attempt, lockout, session revocation, and password reset is recorded with IP address and user agent
- **Session revocation** — admin can invalidate all active sessions for a user
- **Force password reset** — admin can flag any account

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit your changes: `git commit -m "Add my feature"`
4. Push and open a Pull Request

---

## 📄 License

This project is for educational and demonstration purposes.
