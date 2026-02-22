#!/usr/bin/env python3
"""SSL via Certbot standalone mode (stops IIS temporarily)"""

import paramiko
import time

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
DOMAIN = "learning.prospergenics.com"
EMAIL = "martien@prospergenics.com"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 70)
print("  SSL via Certbot Standalone")
print("=" * 70)

# Step 1: Install Certbot via Chocolatey
print("\n[1/7] Installing Certbot...")
install_cmd = 'powershell -Command "choco install certbot -y --force"'
stdin, stdout, stderr = ssh.exec_command(install_cmd, timeout=120)
exit_code = stdout.channel.recv_exit_status()

if exit_code == 0:
    print("  [OK] Certbot installed")
else:
    print("  [INFO] Certbot may already be installed")

time.sleep(2)

# Step 2: Find certbot executable
print("\n[2/7] Finding Certbot...")
find_cmd = r'where certbot'
stdin, stdout, stderr = ssh.exec_command(find_cmd)
certbot_path = stdout.read().decode().strip()

if not certbot_path:
    certbot_path = r'C:\ProgramData\chocolatey\bin\certbot.exe'

print(f"  Certbot path: {certbot_path}")

# Step 3: Stop IIS temporarily
print("\n[3/7] Stopping IIS (temporary)...")
stdin, stdout, stderr = ssh.exec_command('iisreset /stop')
stop_output = stdout.read().decode()
print(f"  {stop_output.strip()}")

time.sleep(3)

# Step 4: Request certificate in standalone mode
print(f"\n[4/7] Requesting certificate for {DOMAIN}...")
print("  (Certbot will start temporary server on port 80)")

cert_cmd = f'"{certbot_path}" certonly --standalone -d {DOMAIN} --email {EMAIL} --agree-tos --non-interactive --preferred-challenges http'

stdin, stdout, stderr = ssh.exec_command(cert_cmd, timeout=120)
cert_output = stdout.read().decode('utf-8', errors='ignore')
cert_error = stderr.read().decode('utf-8', errors='ignore')

print("  Output:")
for line in cert_output.split('\n')[-15:]:
    if line.strip():
        print(f"    {line}")

if "Successfully received certificate" in cert_output:
    print("  [OK] Certificate obtained!")
elif "Congratulations" in cert_output:
    print("  [OK] Certificate obtained!")
else:
    print(f"  [WARN] Check output above")

# Step 5: Start IIS again
print("\n[5/7] Starting IIS...")
stdin, stdout, stderr = ssh.exec_command('iisreset /start')
start_output = stdout.read().decode()
print(f"  {start_output.strip()}")

time.sleep(3)

# Step 6: Import certificate to IIS
print("\n[6/7] Importing certificate to IIS...")
import_cmd = f'''powershell -Command "
# Find certificate files
$certPath = 'C:\\Certbot\\live\\{DOMAIN}\\fullchain.pem'
$keyPath = 'C:\\Certbot\\live\\{DOMAIN}\\privkey.pem'

if (Test-Path $certPath) {{
    # Convert PEM to PFX
    $pfxPath = 'C:\\Certbot\\live\\{DOMAIN}\\cert.pfx'
    $password = ConvertTo-SecureString -String 'certpass' -Force -AsPlainText

    # Use OpenSSL if available, otherwise skip
    $opensslPath = 'C:\\Program Files\\Git\\usr\\bin\\openssl.exe'
    if (Test-Path $opensslPath) {{
        & $opensslPath pkcs12 -export -out $pfxPath -inkey $keyPath -in $certPath -password pass:certpass

        # Import PFX
        $cert = Import-PfxCertificate -FilePath $pfxPath -CertStoreLocation Cert:\\LocalMachine\\My -Password $password

        Write-Output \\"Certificate imported: $($cert.Thumbprint)\\"
        $cert.Thumbprint
    }} else {{
        Write-Output \\"OpenSSL not found - manual import needed\\"
    }}
}} else {{
    Write-Output \\"Certificate files not found at $certPath\\"
}}
"'''

stdin, stdout, stderr = ssh.exec_command(import_cmd, timeout=60)
import_output = stdout.read().decode()
print(f"  {import_output}")

# Extract thumbprint
thumbprint = None
for line in import_output.split('\n'):
    if len(line.strip()) == 40 and all(c in '0123456789ABCDEFabcdef' for c in line.strip()):
        thumbprint = line.strip()
        break

# Step 7: Bind to IIS
if thumbprint:
    print(f"\n[7/7] Binding certificate to IIS...")
    bind_cmd = f'''powershell -Command "
Import-Module WebAdministration

# Remove old HTTPS binding
Get-WebBinding -Name LearningToolApp -Protocol https -ErrorAction SilentlyContinue | Remove-WebBinding

# Create new HTTPS binding
New-WebBinding -Name LearningToolApp -Protocol https -Port 443 -HostHeader '{DOMAIN}' -SslFlags 1

# Bind certificate
$binding = Get-WebBinding -Name LearningToolApp -Protocol https
$binding.AddSslCertificate('{thumbprint}', 'My')

Write-Output \\"Certificate bound to IIS\\"
"'''

    stdin, stdout, stderr = ssh.exec_command(bind_cmd)
    bind_output = stdout.read().decode()
    print(f"  {bind_output.strip()}")

# Test
print(f"\nTesting HTTPS...")
stdin, stdout, stderr = ssh.exec_command(f'powershell -Command "try {{ (Invoke-WebRequest -Uri https://{DOMAIN} -UseBasicParsing -TimeoutSec 10).StatusCode }} catch {{ Write-Output $_.Exception.Message }}"', timeout=15)
result = stdout.read().decode().strip()
print(f"Result: {result}")

ssh.close()

print("\n" + "=" * 70)
if "200" in result:
    print("  SUCCESS! HTTPS IS ACTIVE!")
    print("=" * 70)
    print(f"\n  https://{DOMAIN}")
else:
    print("  Certificate installed - manual verification needed")
    print("=" * 70)
    print(f"\n  Test: https://{DOMAIN}")
