using AgileTaskManager.Models.ViewModels;

namespace AgileTaskManager.Services.Interfaces;

public interface ISecurityService
{
    Task<bool> IsAccountLockedAsync(string userId);
    Task<bool> VerifyTwoFactorCodeAsync(string userId, string code);
    Task<bool> EnableTwoFactorAsync(string userId);
    Task<bool> DisableTwoFactorAsync(string userId);
    Task<string> GenerateTwoFactorCodeAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> ValidatePasswordStrengthAsync(string password);
    Task<SecurityActivityViewModel> GetRecentSecurityActivityAsync(string userId, int count = 10);
    Task<bool> IsSuspiciousActivityDetectedAsync(string userId);
    Task<bool> LockAccountAsync(string userId, string reason, int? durationMinutes = null);
    Task<bool> UnlockAccountAsync(string userId, string reason);
    Task<SecuritySettingsViewModel> GetSecuritySettingsAsync(string userId);
    Task<SecurityAuditViewModel> GetSecurityAuditAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> RevokeAllSessionsAsync(string userId);
    Task<List<LoginAttemptViewModel>> GetRecentLoginAttemptsAsync(string userId, int count = 20);
    Task<bool> IsPasswordExpiredAsync(string userId);
    Task<bool> ForcePasswordResetAsync(string userId, string reason);
    Task<PasswordPolicyViewModel> GetPasswordPolicyAsync();
}
