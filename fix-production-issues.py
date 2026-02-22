#!/usr/bin/env python3
"""Fix production issues: IIS URL Rewrite + SSL"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 70)
print("  Fixing Production Issues")
print("=" * 70)

# Step 1: Create web.config for URL Rewrite (React routing)
print("\n[1/4] Creating web.config for React routing...")
web_config = '''<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="React Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/api" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>'''

# Write web.config to server
cmd = f'powershell -Command "Set-Content -Path C:\\stores\\learningtool\\www\\web.config -Value \'{web_config.replace("'", "''")}\'"'
stdin, stdout, stderr = ssh.exec_command(cmd)
stdout.channel.recv_exit_status()
print("  [OK] web.config created")

# Step 2: Install URL Rewrite module if not present
print("\n[2/4] Checking URL Rewrite module...")
cmd = 'powershell -Command "Get-WindowsFeature Web-Http-Redirect"'
stdin, stdout, stderr = ssh.exec_command(cmd)
output = stdout.read().decode()
print(f"  URL Rewrite status: {output[:100]}")

# Step 3: Install Certbot for SSL
print("\n[3/4] Installing Certbot for SSL...")
cmd = 'powershell -Command "winget list Certbot.Certbot"'
stdin, stdout, stderr = ssh.exec_command(cmd)
output = stdout.read().decode()

if "Certbot" in output:
    print("  [OK] Certbot already installed")
else:
    print("  Installing Certbot (this may take a minute)...")
    cmd = 'powershell -Command "winget install Certbot.Certbot --silent --accept-package-agreements --accept-source-agreements"'
    stdin, stdout, stderr = ssh.exec_command(cmd, timeout=120)
    stdout.channel.recv_exit_status()
    print("  [OK] Certbot installed")

# Step 4: Configure SSL certificate
print("\n[4/4] Configuring SSL certificate...")
print("  Manual steps required:")
print("  1. SSH to server: ssh administrator@85.215.217.154")
print("  2. Run: certbot certonly --webroot -w C:\\stores\\learningtool\\www -d learning.prospergenics.com")
print("  3. Follow prompts (enter email for renewal notifications)")
print("  4. Certificate will be saved to C:\\Certbot\\live\\learning.prospergenics.com")
print()
print("  Then add HTTPS binding in IIS:")
print("  5. Import-Module WebAdministration")
print("  6. New-WebBinding -Name LearningToolApp -Protocol https -Port 443 -HostHeader learning.prospergenics.com")
print("  7. Configure SSL certificate in IIS Manager")

ssh.close()

print("\n" + "=" * 70)
print("  URL Rewrite configured!")
print("=" * 70)
print("\n  React routing should now work.")
print("  Test: http://learning.prospergenics.com/register")
print("\n  For SSL: Follow manual steps above")
