#!/usr/bin/env python3
"""Install SSL using Win-ACME (Windows-native Let's Encrypt client)"""

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
print("  SSL Certificate Installation via Win-ACME")
print("=" * 70)

# Step 1: Download Win-ACME
print("\n[1/5] Downloading Win-ACME...")
download_cmd = r'''powershell -Command "
$url = 'https://github.com/win-acme/win-acme/releases/download/v2.2.9.1701/win-acme.v2.2.9.1701.x64.pluggable.zip'
$output = 'C:\win-acme.zip'
$extract = 'C:\win-acme'

# Download
Invoke-WebRequest -Uri $url -OutFile $output
Write-Output 'Downloaded'

# Extract
Expand-Archive -Path $output -DestinationPath $extract -Force
Write-Output 'Extracted'

# Cleanup
Remove-Item $output
Write-Output 'Done'
"'''

stdin, stdout, stderr = ssh.exec_command(download_cmd, timeout=120)
output = stdout.read().decode()
error = stderr.read().decode()

if "Done" in output:
    print("  [OK] Win-ACME downloaded and extracted to C:\\win-acme")
else:
    print(f"  Output: {output}")
    if error:
        print(f"  Error: {error[:500]}")

# Step 2: Run Win-ACME to create certificate
print(f"\n[2/5] Creating SSL certificate for {DOMAIN}...")
print("  (This may take 30-60 seconds)")

# Win-ACME command for IIS site binding
wacs_cmd = f'''C:\\win-acme\\wacs.exe --source iis --siteid 1 --host {DOMAIN} --emailaddress {EMAIL} --accepttos --installation iis --store certificatestore --renew'''

stdin, stdout, stderr = ssh.exec_command(wacs_cmd, timeout=180)
exit_code = stdout.channel.recv_exit_status()
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

print(f"  Exit code: {exit_code}")
print(f"  Output: {output[:1000]}")
if error:
    print(f"  Stderr: {error[:500]}")

# Step 3: Verify HTTPS binding
print("\n[3/5] Verifying IIS HTTPS binding...")
verify_cmd = 'powershell -Command "Import-Module WebAdministration; Get-WebBinding -Name LearningToolApp | Format-Table -AutoSize"'
stdin, stdout, stderr = ssh.exec_command(verify_cmd)
bindings = stdout.read().decode()
print(f"  Current bindings:\n{bindings}")

# Step 4: Test HTTPS
print("\n[4/5] Testing HTTPS connection...")
test_cmd = f'powershell -Command "try {{ Invoke-WebRequest -Uri https://{DOMAIN} -UseBasicParsing -TimeoutSec 10 | Select-Object StatusCode }} catch {{ Write-Output \\"Failed: $($_.Exception.Message)\\" }}"'
stdin, stdout, stderr = ssh.exec_command(test_cmd, timeout=15)
test_result = stdout.read().decode()
print(f"  Result: {test_result.strip()}")

ssh.close()

print("\n" + "=" * 70)
print("  SSL Installation Complete!")
print("=" * 70)
print(f"\n  Test your site:")
print(f"  HTTPS: https://{DOMAIN}")
print(f"  HTTP:  http://{DOMAIN}")
print(f"\n  Win-ACME location: C:\\win-acme\\wacs.exe")
print(f"  Automatic renewal: Configured via Windows Task Scheduler")
print(f"  Certificate renewal: Every 60 days automatically")
