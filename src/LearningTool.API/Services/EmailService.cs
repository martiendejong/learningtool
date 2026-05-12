using System.Net;
using System.Net.Mail;

namespace LearningTool.API.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger, IWebHostEnvironment env)
{
    public async Task SendOtpAsync(string toEmail, string code)
    {
        var smtp = config.GetSection("Smtp");
        var host = smtp["Host"];
        var port = int.Parse(smtp["Port"] ?? "587");
        var username = smtp["Username"];
        var password = smtp["Password"];
        var from = smtp["From"] ?? username;

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            if (env.IsDevelopment())
                logger.LogWarning("SMTP not configured — OTP for {Email} is: {Code}", toEmail, code);
            else
                logger.LogError("SMTP is not configured. Cannot send OTP email to {Email}", toEmail);
            return;
        }

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        var body = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:auto;padding:32px">
              <h2 style="color:#101f3c;margin-bottom:8px">Verify your email</h2>
              <p style="color:#6b7280;margin-bottom:24px">
                Use the code below to complete your LearningTool sign-up.
                It expires in <strong>10 minutes</strong>.
              </p>
              <div style="font-size:36px;font-weight:700;letter-spacing:12px;color:#4f46e5;
                          background:#eef2ff;border-radius:8px;padding:20px 32px;text-align:center">
                {code}
              </div>
              <p style="color:#9ca3af;font-size:12px;margin-top:24px">
                If you didn't sign up for LearningTool, you can safely ignore this email.
              </p>
            </div>
            """;

        var message = new MailMessage
        {
            From = new MailAddress(from!, "LearningTool"),
            Subject = $"Your LearningTool verification code: {code}",
            Body = body,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
        logger.LogInformation("OTP sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetAsync(string toEmail, string resetUrl)
    {
        var smtp = config.GetSection("Smtp");
        var host = smtp["Host"];
        var port = int.Parse(smtp["Port"] ?? "587");
        var username = smtp["Username"];
        var password = smtp["Password"];
        var from = smtp["From"] ?? username;

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            if (env.IsDevelopment())
                logger.LogWarning("SMTP not configured — password reset URL for {Email}: {Url}", toEmail, resetUrl);
            else
                logger.LogError("SMTP is not configured. Cannot send reset email to {Email}", toEmail);
            return;
        }

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        var body = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:auto;padding:32px">
              <h2 style="color:#101f3c;margin-bottom:8px">Reset your password</h2>
              <p style="color:#6b7280;margin-bottom:24px">
                We received a request to reset your password. Click the button below.
                The link expires in <strong>1 hour</strong>.
              </p>
              <a href="{resetUrl}"
                 style="display:inline-block;background:#16a34a;color:#fff;font-weight:600;
                        padding:12px 28px;border-radius:8px;text-decoration:none">
                Reset Password
              </a>
              <p style="color:#9ca3af;font-size:12px;margin-top:24px">
                If you didn't request this, you can safely ignore this email.
                Your password will not change.
              </p>
            </div>
            """;

        var message = new MailMessage
        {
            From = new MailAddress(from!, "LearningTool"),
            Subject = "Reset your LearningTool password",
            Body = body,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
        logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }
}
