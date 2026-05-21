using System.ComponentModel.DataAnnotations;

namespace AgileTaskManager.Models.ViewModels;

public class SecurityActivityViewModel
{
    public List<SecurityEvent> Activities { get; set; } = new();
}

public class SecurityEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public class SecuritySettingsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public bool TwoFactorEnabled { get; set; }
    public bool PasswordChangeRequired { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public bool AccountLocked { get; set; }
    public List<SecurityQuestionViewModel> SecurityQuestions { get; set; } = new();
    public int SessionTimeout { get; set; }
    public int MaxLoginAttempts { get; set; }
    public int PasswordExpiryDays { get; set; }
}

public class SecurityQuestionViewModel
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string AnswerHash { get; set; } = string.Empty;
}

public class SecurityAuditViewModel
{
    public string UserId { get; set; } = string.Empty;
    public List<UserSecurityLog> Logs { get; set; } = new();
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TotalCount { get; set; }
}

public class UserSecurityLog
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? AdditionalData { get; set; }
}

public class LoginAttemptViewModel
{
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}

public class PasswordPolicyViewModel
{
    [Display(Name = "Minimum Length")]
    public int MinLength { get; set; } = 8;
    
    [Display(Name = "Require Uppercase")]
    public bool RequireUppercase { get; set; } = true;
    
    [Display(Name = "Require Lowercase")]
    public bool RequireLowercase { get; set; } = true;
    
    [Display(Name = "Require Digit")]
    public bool RequireDigit { get; set; } = true;
    
    [Display(Name = "Require Special Character")]
    public bool RequireSpecialCharacter { get; set; } = true;
    
    [Display(Name = "Maximum Length")]
    public int MaxLength { get; set; } = 128;
    
    [Display(Name = "Prevent Reuse")]
    public bool PreventReuse { get; set; } = true;
    
    [Display(Name = "Password History")]
    public int HistoryCount { get; set; } = 5;
    
    [Display(Name = "Expiry Days")]
    public int ExpiryDays { get; set; } = 90;
}

public class VerifyTwoFactorViewModel
{
    [Required]
    public string Code { get; set; } = string.Empty;
}

public class ChangePasswordViewModel
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class LockAccountViewModel
{
    [Required]
    public string Reason { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
}

public class UnlockAccountViewModel
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}

public class ForcePasswordResetViewModel
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}
