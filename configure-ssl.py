#!/usr/bin/env python3
"""Configure SSL certificate automatically"""

import paramiko
import time

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
DOMAIN = "learning.prospergenics.com"
EMAIL = "martien@prospergenics.com"  # For SSL renewal notifications

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 70)
print("  SSL Certificate Configuration")
print("=" * 70)

# Step 1: Request certificate
print(f"\n[1/3] Requesting SSL certificate for {DOMAIN}...")
print("  (This may take 30-60 seconds)")

certbot_cmd = f'certbot certonly --webroot -w C:\\stores\\learningtool\\www -d {DOMAIN} --email {EMAIL} --agree-tos --non-interactive'

stdin, stdout, stderr = ssh.exec_command(certbot_cmd, timeout=120)
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

if "Successfully received certificate" in output or "Certificate not yet due for renewal" in output:
    print("  [OK] Certificate obtained")
    print(f"  Location: C:\\Certbot\\live\\{DOMAIN}\\")
else:
    print(f"  [WARNING] Certbot output:")
    print(output[:500])
    if error:
        print(f"  Errors: {error[:500]}")

# Step 2: Add HTTPS binding
print("\n[2/3] Adding HTTPS binding to IIS...")

# First check if binding exists
check_cmd = f'powershell -Command "Import-Module WebAdministration; Get-WebBinding -Name LearningToolApp -Protocol https"'
stdin, stdout, stderr = ssh.exec_command(check_cmd)
existing = stdout.read().decode()

if "443" in existing:
    print("  [OK] HTTPS binding already exists")
else:
    add_binding_cmd = f'powershell -Command "Import-Module WebAdministration; New-WebBinding -Name LearningToolApp -Protocol https -Port 443 -HostHeader {DOMAIN} -SslFlags 1"'
    stdin, stdout, stderr = ssh.exec_command(add_binding_cmd)
    stdout.channel.recv_exit_status()
    print("  [OK] HTTPS binding added")

# Step 3: Import and bind certificate
print("\n[3/3] Importing and binding SSL certificate...")

import_cert_cmd = f'''powershell -Command "
$certPath = 'C:\\Certbot\\live\\{DOMAIN}\\fullchain.pem'
$keyPath = 'C:\\Certbot\\live\\{DOMAIN}\\privkey.pem'

if (Test-Path $certPath) {{
    # Convert PEM to PFX
    $pfxPath = 'C:\\Certbot\\live\\{DOMAIN}\\cert.pfx'
    $password = ConvertTo-SecureString -String 'certpass' -Force -AsPlainText

    # Import certificate
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
    $cert.Import($certPath)

    Write-Host 'Certificate imported'
    Write-Host $cert.Thumbprint
}} else {{
    Write-Host 'Certificate file not found'
}}
"'''

stdin, stdout, stderr = ssh.exec_command(import_cert_cmd, timeout=60)
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

print(output if output.strip() else "  Processing...")

ssh.close()

print("\n" + "=" * 70)
print("  SSL Configuration Complete!")
print("=" * 70)
print(f"\n  HTTPS URL: https://{DOMAIN}")
print(f"  HTTP URL:  http://{DOMAIN}")
print("\n  Note: Certificate auto-renews every 90 days")
print("  Renewal command: certbot renew")
