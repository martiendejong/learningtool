#!/usr/bin/env python3
"""Send getting started email to Diko Mohamed"""

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

# Credentials
URL = "http://learning.prospergenics.com"
EMAIL = "dikomohamed287@gmail.com"
PASSWORD = "PRNOcv6IW@r*Ka!8"
GITHUB_REPO = "https://github.com/martiendejong/learningtool"

# Create message
msg = MIMEMultipart('alternative')
msg['Subject'] = "Getting Started with LearningTool Development"
msg['From'] = f"{FROM_NAME} <{FROM_EMAIL}>"
msg['To'] = f"{TO_NAME} <{TO_EMAIL}>"

# Plain text version
text = f"""Hi Diko,

Welcome to the LearningTool project! Here's everything you need to get started.

===== YOUR ACCESS =====

Platform Login:
URL: {URL}
Email: {EMAIL}
Password: {PASSWORD}

GitHub Repository:
{GITHUB_REPO}

Note: You'll receive a GitHub collaboration invite separately. Accept it to get push access to the repository.


===== QUICK START GUIDE =====

1. LOGIN TO THE PLATFORM
   - Go to {URL}
   - Sign in with your credentials above
   - Explore the AI-powered learning features

2. CLONE THE REPOSITORY
   git clone {GITHUB_REPO}.git
   cd learningtool

3. SETUP DEVELOPMENT ENVIRONMENT

   Backend (ASP.NET Core 8.0):
   - Install .NET 8 SDK
   - cd src/LearningTool.API
   - dotnet restore
   - dotnet run
   - API will run on http://localhost:5028

   Frontend (React + Vite):
   - Install Node.js 18+
   - cd frontend
   - npm install
   - npm run dev
   - Frontend will run on http://localhost:5173

4. CONFIGURATION
   - Create appsettings.Secrets.json (see appsettings.example.json)
   - Get API keys from GitHub Secrets (need repository access first)
   - Ask Martien for OpenAI API key if needed


===== PROJECT STRUCTURE =====

learningtool/
  ├── src/
  │   ├── LearningTool.API/         # Web API (ASP.NET Core)
  │   ├── LearningTool.Application/ # Business logic
  │   ├── LearningTool.Domain/      # Domain models
  │   └── LearningTool.Infrastructure/ # Data access
  │
  ├── frontend/
  │   ├── src/
  │   │   ├── components/  # React components
  │   │   ├── pages/       # Page components
  │   │   └── services/    # API services
  │   └── package.json
  │
  └── README.md            # Full documentation


===== KEY FEATURES =====

- AI-powered chat using GPT-4o-mini
- Course creation and management
- Skill tracking system
- Topic-based learning paths
- Text-to-speech support
- Voice input capability
- Google OAuth authentication


===== TECH STACK =====

Backend:
- ASP.NET Core 8.0
- Entity Framework Core (SQLite)
- OpenAI GPT-4o-mini integration
- JWT authentication

Frontend:
- React 18
- TypeScript
- Vite
- Tailwind CSS (or your preferred styling)


===== DEPLOYMENT =====

Production Server: 85.215.217.154
Deployed URL: {URL}
IIS Site: LearningToolApp

Automated deployment via SSH:
  python deploy-complete.py


===== NEXT STEPS =====

1. Accept GitHub collaboration invite
2. Clone the repository
3. Set up local development environment
4. Test the application on {URL}
5. Start exploring the codebase
6. Ask Martien if you have any questions


===== DOCUMENTATION =====

- README.md - Full project documentation
- DEPLOYMENT_SUCCESS.md - Deployment details
- README-DEPLOYMENT.md - Deployment guide
- API Swagger: {URL}/api/swagger/index.html


===== CONTACT =====

For questions or help:
- Email: martien@prospergenics.com
- GitHub: Review pull requests and issues


Looking forward to working with you on this project!

Best regards,
Martien de Jong
Prospergenics
"""

# HTML version
html = f"""
<html>
<body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
    <h2 style="color: #2c3e50;">Welcome to LearningTool Development!</h2>

    <p>Hi Diko,</p>

    <p>Welcome to the LearningTool project! Here's everything you need to get started.</p>

    <div style="background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0;">
        <h3 style="margin-top: 0;">Your Access</h3>

        <p><strong>Platform Login:</strong></p>
        <ul>
            <li><strong>URL:</strong> <a href="{URL}">{URL}</a></li>
            <li><strong>Email:</strong> {EMAIL}</li>
            <li><strong>Password:</strong> <code style="background-color: #e9ecef; padding: 2px 6px; border-radius: 3px;">{PASSWORD}</code></li>
        </ul>

        <p><strong>GitHub Repository:</strong></p>
        <ul>
            <li><a href="{GITHUB_REPO}">{GITHUB_REPO}</a></li>
            <li>You'll receive a collaboration invite separately</li>
        </ul>
    </div>

    <h3>Quick Start Guide</h3>

    <h4>1. Login to the Platform</h4>
    <ul>
        <li>Go to <a href="{URL}">{URL}</a></li>
        <li>Sign in and explore the AI-powered learning features</li>
    </ul>

    <h4>2. Clone the Repository</h4>
    <pre style="background-color: #f5f5f5; padding: 10px; border-radius: 5px;">git clone {GITHUB_REPO}.git
cd learningtool</pre>

    <h4>3. Setup Development Environment</h4>

    <p><strong>Backend (ASP.NET Core 8.0):</strong></p>
    <pre style="background-color: #f5f5f5; padding: 10px; border-radius: 5px;">cd src/LearningTool.API
dotnet restore
dotnet run
# API runs on http://localhost:5028</pre>

    <p><strong>Frontend (React + Vite):</strong></p>
    <pre style="background-color: #f5f5f5; padding: 10px; border-radius: 5px;">cd frontend
npm install
npm run dev
# Frontend runs on http://localhost:5173</pre>

    <h4>4. Configuration</h4>
    <ul>
        <li>Create <code>appsettings.Secrets.json</code> (see appsettings.example.json)</li>
        <li>Get API keys from GitHub Secrets (after repository access)</li>
        <li>Ask Martien for OpenAI API key if needed</li>
    </ul>

    <h3>Key Features</h3>
    <ul>
        <li>AI-powered chat using GPT-4o-mini</li>
        <li>Course creation and management</li>
        <li>Skill tracking system</li>
        <li>Topic-based learning paths</li>
        <li>Text-to-speech & voice input</li>
        <li>Google OAuth authentication</li>
    </ul>

    <h3>Tech Stack</h3>
    <p><strong>Backend:</strong> ASP.NET Core 8.0, EF Core, SQLite, OpenAI, JWT</p>
    <p><strong>Frontend:</strong> React 18, TypeScript, Vite</p>

    <h3>Documentation</h3>
    <ul>
        <li>README.md - Full project documentation</li>
        <li>DEPLOYMENT_SUCCESS.md - Deployment details</li>
        <li>API Swagger: <a href="{URL}/api/swagger/index.html">{URL}/api/swagger/index.html</a></li>
    </ul>

    <div style="background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0;">
        <h4 style="margin-top: 0;">Next Steps</h4>
        <ol>
            <li>Accept GitHub collaboration invite</li>
            <li>Clone the repository</li>
            <li>Set up local development</li>
            <li>Test the application</li>
            <li>Start exploring the codebase</li>
        </ol>
    </div>

    <p>For questions or help, contact: <a href="mailto:martien@prospergenics.com">martien@prospergenics.com</a></p>

    <p>Looking forward to working with you!</p>

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
print(f"Sending getting started email to {TO_EMAIL}...")
print(f"From: {FROM_NAME} <{FROM_EMAIL}>")
print(f"Subject: {msg['Subject']}")
print()

try:
    with smtplib.SMTP_SSL(SMTP_HOST, SMTP_PORT) as server:
        server.login(SMTP_USER, SMTP_PASS)
        server.send_message(msg)

    print("[OK] Email sent successfully!")
    print()
    print("Email includes:")
    print(f"  - Platform login: {URL}")
    print(f"  - GitHub repo: {GITHUB_REPO}")
    print("  - Quick start guide")
    print("  - Development setup instructions")
    print("  - Tech stack overview")
    print("  - Next steps")

except Exception as e:
    print(f"[FAIL] Failed to send email: {e}")
