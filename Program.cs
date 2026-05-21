using AgileTaskManager.Data;
using AgileTaskManager.Hubs;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Services;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Constants;
using AgileTaskManager.Middleware;
using AgileTaskManager.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Configure query splitting to avoid cartesian product warnings
    options.ConfigureWarnings(w => 
    {
        w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning);
        // Only in development, ignore pending model changes warning to allow running without migrations
        if (builder.Environment.IsDevelopment())
        {
            w.Ignore(RelationalEventId.PendingModelChangesWarning);
        }
    });
});

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase       = true;
    options.SignIn.RequireConfirmedAccount  = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Http Context ───────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Additional Services ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddHttpClient<IAIAssistantService, AIAssistantService>();

// ── Tenant Context ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITenantContext, TenantContext>();




// ── Application Services ───────────────────────────────────────────────────────
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISprintService, SprintService>();
builder.Services.AddScoped<ITaskDependencyService, TaskDependencyService>();
builder.Services.AddScoped<ITimeTrackingService, TimeTrackingService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<ICacheService, CacheService>();


// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(typeof(Program));

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();


// ── Authorization ───────────────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(RoleConstants.Admin, policy => 
        policy.RequireRole(RoleConstants.Admin));
    
    options.AddPolicy("ClientOrAbove", policy => 
        policy.RequireRole(RoleConstants.Admin, RoleConstants.Client));
    
    options.AddPolicy("DeveloperOrAbove", policy => 
        policy.RequireRole(RoleConstants.Admin, RoleConstants.Developer, RoleConstants.TeamLead));
});


// ── MVC ───────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ── Cookie config ─────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath    = "/Account/Login";
    options.LogoutPath   = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseMiddleware<ErrorHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

// ── Routes ────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// ── SignalR Hub ───────────────────────────────────────────────────────────────
app.MapHub<TaskHub>("/taskHub").RequireAuthorization();
app.MapHub<AchievementHub>("/achievementHub").RequireAuthorization();


// ── Apply migrations & seed ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        await DatabaseMigrationRepair.TryRecoverBrokenSchemaAsync(context, environment, logger);
        await DatabaseMigrationRepair.TryStampInitialMigrationIfSchemaExistsAsync(context, logger);
        await context.Database.MigrateAsync();
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        scope.ServiceProvider.GetRequiredService<ILogger<Program>>()
            .LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();
