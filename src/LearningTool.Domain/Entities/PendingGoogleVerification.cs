namespace LearningTool.Domain.Entities;

public class PendingGoogleVerification
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string GoogleId { get; set; } = string.Empty;
    /// <summary>Plain-text 6-digit OTP. Short-lived — expires after 10 minutes.</summary>
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
