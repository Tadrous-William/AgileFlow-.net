using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AgileTaskManager.Services;

public class SecurityService : ISecurityService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SecurityService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<bool> IsAccountLockedAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.IsLockedOutAsync(user);
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        return await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultProvider, code);
    }

    public async Task<bool> EnableTwoFactorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        return result.Succeeded;
    }

    public async Task<bool> DisableTwoFactorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        return result.Succeeded;
    }

    public async Task<string> GenerateTwoFactorCodeAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return string.Empty;

        return await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultProvider);
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ValidatePasswordStrengthAsync(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    public async Task<SecurityActivityViewModel> GetRecentSecurityActivityAsync(string userId, int count = 10)
    {
        var logs = await _db.UserSecurityLogs
            .Where(usl => usl.UserId == userId)
            .OrderByDescending(usl => usl.Timestamp)
            .Take(count)
            .ToListAsync();

        return new SecurityActivityViewModel
        {
            Activities = logs.Select(usl => new SecurityEvent
            {
                EventType = usl.EventType,
                Description = usl.Description,
                Timestamp = usl.Timestamp,
                Success = true,
                IpAddress = usl.IpAddress,
                UserAgent = usl.UserAgent
            }).ToList()
        };
    }

    public async Task<bool> IsSuspiciousActivityDetectedAsync(string userId)
    {
        var recentLogs = await _db.UserSecurityLogs
            .Where(usl => usl.UserId == userId)
            .Where(usl => usl.Timestamp > DateTime.UtcNow.AddHours(-1))
            .ToListAsync();

        // Check for multiple failed login attempts
        var failedAttempts = recentLogs.Count(l => l.EventType == "LoginFailed");
        return failedAttempts > 5;
    }

    public async Task<bool> LockAccountAsync(string userId, string reason, int? durationMinutes = null)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, durationMinutes.HasValue 
            ? DateTimeOffset.UtcNow.AddMinutes(durationMinutes.Value) 
            : DateTimeOffset.MaxValue);

        await _db.UserSecurityLogs.AddAsync(new Models.Entities.UserSecurityLog
        {
            UserId = userId,
            EventType = "AccountLocked",
            Description = $"Account locked: {reason}",
            Timestamp = DateTime.UtcNow,
            TenantId = 1
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnlockAccountAsync(string userId, string reason)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);

        await _db.UserSecurityLogs.AddAsync(new Models.Entities.UserSecurityLog
        {
            UserId = userId,
            EventType = "AccountUnlocked",
            Description = $"Account unlocked: {reason}",
            Timestamp = DateTime.UtcNow,
            TenantId = 1
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<SecuritySettingsViewModel> GetSecuritySettingsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        return new SecuritySettingsViewModel
        {
            UserId = userId,
            TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            PasswordChangeRequired = false,
            LastPasswordChange = null,
            AccountLocked = await _userManager.IsLockedOutAsync(user),
            SecurityQuestions = new List<SecurityQuestionViewModel>(),
            SessionTimeout = 30,
            MaxLoginAttempts = 5,
            PasswordExpiryDays = 90
        };
    }

    public async Task<SecurityAuditViewModel> GetSecurityAuditAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _db.UserSecurityLogs.Where(usl => usl.UserId == userId);
        
        if (fromDate.HasValue)
            query = query.Where(usl => usl.Timestamp >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(usl => usl.Timestamp <= toDate.Value);

        var logs = await query.ToListAsync();

        return new SecurityAuditViewModel
        {
            UserId = userId,
            Logs = logs.Select(l => new Models.ViewModels.UserSecurityLog
            {
                Id = l.Id,
                EventType = l.EventType,
                Description = l.Description,
                Timestamp = l.Timestamp,
                IpAddress = l.IpAddress,
                UserAgent = l.UserAgent,
                AdditionalData = l.AdditionalData
            }).ToList(),
            FromDate = fromDate,
            ToDate = toDate,
            TotalCount = logs.Count
        };
    }

    public async Task<bool> RevokeAllSessionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        await _userManager.UpdateSecurityStampAsync(user);

        await _db.UserSecurityLogs.AddAsync(new Models.Entities.UserSecurityLog
        {
            UserId = userId,
            EventType = "SessionsRevoked",
            Description = "All sessions revoked",
            Timestamp = DateTime.UtcNow,
            TenantId = 1
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<LoginAttemptViewModel>> GetRecentLoginAttemptsAsync(string userId, int count = 20)
    {
        var logs = await _db.UserSecurityLogs
            .Where(usl => usl.UserId == userId && usl.EventType.StartsWith("Login"))
            .OrderByDescending(usl => usl.Timestamp)
            .Take(count)
            .ToListAsync();

        return logs.Select(usl => new LoginAttemptViewModel
        {
            Timestamp = usl.Timestamp,
            IpAddress = usl.IpAddress,
            UserAgent = usl.UserAgent,
            Success = usl.EventType == "LoginSuccess",
            FailureReason = usl.EventType == "LoginFailed" ? usl.Description : null
        }).ToList();
    }

    public async Task<bool> IsPasswordExpiredAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        return false; // Password expiry not implemented
    }

    public async Task<bool> ForcePasswordResetAsync(string userId, string reason)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        await _db.UserSecurityLogs.AddAsync(new Models.Entities.UserSecurityLog
        {
            UserId = userId,
            EventType = "PasswordResetForced",
            Description = $"Password reset forced: {reason}",
            Timestamp = DateTime.UtcNow,
            TenantId = 1
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PasswordPolicyViewModel> GetPasswordPolicyAsync()
    {
        return new PasswordPolicyViewModel
        {
            MinLength = 8,
            RequireUppercase = true,
            RequireLowercase = true,
            RequireDigit = true,
            RequireSpecialCharacter = true,
            MaxLength = 128,
            PreventReuse = true,
            HistoryCount = 5,
            ExpiryDays = 90
        };
    }
}
