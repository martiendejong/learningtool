#!/usr/bin/env python3
"""Install Certbot via Chocolatey and configure SSL"""

import paramiko
import time

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
DOMAIN = "learning.prospergenics.com"
EMAIL = "martien@prospergenics.com"

def execute_and_show(ssh, command, description):
    """Execute command and show output"""
    print(f"\n{description}")
    print(f"  Command: {command[:100]}...")

    stdin, stdout, stderr = ssh.exec_command(command, timeout=180)
    exit_code = stdout.channel.recv_exit_status()

    output = stdout.read().decode('utf-8', errors='ignore')
    error = stderr.read().decode('utf-8', errors='ignore')

    if output:
        print(f"  Output: {output[:500]}")
    if error and exit_code != 0:
        print(f"  Error: {error[:500]}")

    return exit_code == 0

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 70)
print("  SSL Certificate Installation & Configuration")
print("=" * 70)

# Step 1: Check if Chocolatey is installed
print("\n[1/4] Checking Chocolatey...")
stdin, stdout, stderr = ssh.exec_command('choco --version')
choco_output = stdout.read().decode()

if "Chocolatey" in choco_output:
    print(f"  [OK] Chocolatey installed: {choco_output.strip()}")
else:
    print("  [WARN] Chocolatey not installed, will use alternative method")

# Step 2: Install Certbot via Chocolatey (more reliable on Windows)
print("\n[2/4] Installing/Updating Certbot...")

# Try Chocolatey first
choco_install = execute_and_show(
    ssh,
    'choco install certbot -y --force',
    "  Installing via Chocolatey..."
)

if not choco_install:
    print("  [INFO] Chocolatey install failed/skipped, Certbot may already be installed")

# Refresh environment and find Certbot
print("\n[3/4] Finding Certbot executable...")
find_cmd = r'''powershell -Command "
$paths = @(
    'C:\ProgramData\chocolatey\bin\certbot.exe',
    'C:\Program Files\Certbot\bin\certbot.exe',
    (Get-ChildItem -Path 'C:\Users\administrator\AppData\Local\Microsoft\WinGet\Packages' -Recurse -Filter 'certbot.exe' -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName)
)
foreach ($p in $paths) {
    if (Test-Path $p) {
        Write-Output $p
        break
    }
}
"'''

stdin, stdout, stderr = ssh.exec_command(find_cmd, timeout=60)
certbot_path = stdout.read().decode().strip()

if not certbot_path:
    print("  [FAIL] Could not find Certbot executable")
    print("  You may need to:")
    print("  1. Restart the server to refresh PATH")
    print("  2. Or manually install from: https://certbot.eff.org/")
    ssh.close()
    exit(1)

print(f"  [OK] Found Certbot: {certbot_path}")

# Step 4: Request SSL certificate
print(f"\n[4/4] Requesting SSL certificate for {DOMAIN}...")
print("  (This may take 30-60 seconds)")

certbot_cmd = f'''"{certbot_path}" certonly --webroot -w C:\\stores\\learningtool\\www -d {DOMAIN} --email {EMAIL} --agree-tos --non-interactive --force-renewal'''

stdin, stdout, stderr = ssh.exec_command(certbot_cmd, timeout=120)
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

print(f"  Output: {output[:800]}")
if error:
    print(f"  Stderr: {error[:800]}")

if "Successfully received certificate" in output or "Certificate not yet due for renewal" in output:
    print("\n  [OK] Certificate obtained successfully!")
    print(f"  Location: C:\\Certbot\\live\\{DOMAIN}\\")

    # Add HTTPS binding to IIS
    print("\n[5/5] Configuring IIS HTTPS binding...")

    # Check if HTTPS binding exists
    check_cmd = f'powershell -Command "Import-Module WebAdministration; Get-WebBinding -Name LearningToolApp -Protocol https"'
    stdin, stdout, stderr = ssh.exec_command(check_cmd)
    existing = stdout.read().decode()

    if "443" in existing:
        print("  [OK] HTTPS binding already exists")
    else:
        add_binding_cmd = f'powershell -Command "Import-Module WebAdministration; New-WebBinding -Name LearningToolApp -Protocol https -Port 443 -HostHeader {DOMAIN} -SslFlags 1"'
        stdin, stdout, stderr = ssh.exec_command(add_binding_cmd)
        stdout.channel.recv_exit_status()
        print("  [OK] HTTPS binding added to IIS")

    print("\n" + "=" * 70)
    print("  SSL Configuration Complete!")
    print("=" * 70)
    print(f"\n  HTTPS URL: https://{DOMAIN}")
    print(f"  HTTP URL:  http://{DOMAIN}")
    print("\n  Note: Certificate auto-renews every 90 days")

else:
    print("\n  [WARN] Certificate request may have failed")
    print("  Check the output above for details")

ssh.close()
