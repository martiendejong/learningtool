#!/usr/bin/env python3
"""Send email to Diko Mohamed with LearningTool credentials"""

import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart

# SMTP configuration
SMTP_HOST = "mail.zxcs.nl"
SMTP_PORT = 465
SMTP_USER = "info@prospergenics.com"
SMTP_PASS = "Ku8apzcYtThmRuvredvk"
FROM_NAME = "Martien de Jong"
FROM_EMAIL = "info@prospergenics.com"

# Recipient
TO_EMAIL = "dikomohamed287@gmail.com"
TO_NAME = "Diko Mohamed"

# Credentials from diko-credentials.txt
URL = "http://learning.prospergenics.com"
EMAIL = "dikomohamed287@gmail.com"
PASSWORD = "PRNOcv6IW@r*Ka!8"

# Create message
msg = MIMEMultipart('alternative')
msg['Subject'] = "Your LearningTool Account"
msg['From'] = f"{FROM_NAME} <{FROM_EMAIL}>"
msg['To'] = f"{TO_NAME} <{TO_EMAIL}>"

# Plain text version
text = f"""Hi Diko,

Your LearningTool account has been created!

Login Details:
URL: {URL}
Email: {EMAIL}
Password: {PASSWORD}

Getting Started:
1. Go to {URL}
2. Click "Sign in"
3. Use the email and password above
4. Start exploring the learning platform!

IMPORTANT: Please change your password after your first login for security.

If you have any questions or need help, feel free to reach out.

Best regards,
Martien de Jong
Prospergenics
"""

# HTML version
html = f"""
<html>
<body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
    <h2 style="color: #2c3e50;">Welcome to LearningTool!</h2>

    <p>Hi Diko,</p>

    <p>Your LearningTool account has been created!</p>

    <div style="background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0;">
        <h3 style="margin-top: 0;">Login Details</h3>
        <p style="margin: 5px 0;"><strong>URL:</strong> <a href="{URL}">{URL}</a></p>
        <p style="margin: 5px 0;"><strong>Email:</strong> {EMAIL}</p>
        <p style="margin: 5px 0;"><strong>Password:</strong> <code style="background-color: #e9ecef; padding: 2px 6px; border-radius: 3px;">{PASSWORD}</code></p>
    </div>

    <h3>Getting Started</h3>
    <ol>
        <li>Go to <a href="{URL}">{URL}</a></li>
        <li>Click "Sign in"</li>
        <li>Use the email and password above</li>
        <li>Start exploring the learning platform!</li>
    </ol>

    <div style="background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0;">
        <strong>IMPORTANT:</strong> Please change your password after your first login for security.
    </div>

    <p>If you have any questions or need help, feel free to reach out.</p>

    <p>Best regards,<br>
    <strong>Martien de Jong</strong><br>
    Prospergenics</p>
</body>
</html>
"""

# Attach both versions
part1 = MIMEText(text, 'plain')
part2 = MIMEText(html, 'html')
msg.attach(part1)
msg.attach(part2)

# Send email
print(f"Sending email to {TO_EMAIL}...")
print(f"From: {FROM_NAME} <{FROM_EMAIL}>")
print(f"Subject: {msg['Subject']}")
print()

try:
    # Connect using SSL
    with smtplib.SMTP_SSL(SMTP_HOST, SMTP_PORT) as server:
        server.login(SMTP_USER, SMTP_PASS)
        server.send_message(msg)

    print("[OK] Email sent successfully!")
    print()
    print(f"Diko can now login at: {URL}")
    print(f"Email: {EMAIL}")
    print(f"Password: {PASSWORD}")

except Exception as e:
    print(f"[FAIL] Failed to send email: {e}")
    print()
    print("Credentials are saved in: C:\\Projects\\learningtool\\diko-credentials.txt")
