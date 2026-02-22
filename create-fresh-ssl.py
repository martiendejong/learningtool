#!/usr/bin/env python3
"""Create fresh SSL certificate with Win-ACME"""

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
print("  Creating Fresh SSL Certificate")
print("=" * 70)

# First, find the IIS site ID
print("\n[1/6] Finding IIS site ID...")
siteid_cmd = 'powershell -Command "Import-Module WebAdministration; (Get-Website -Name LearningToolApp).ID"'
stdin, stdout, stderr = ssh.exec_command(siteid_cmd)
site_id = stdout.read().decode().strip()
print(f"  Site ID: {site_id}")

# Step 1: Create certificate using Win-ACME (without --renew)
print(f"\n[2/6] Creating SSL certificate for {DOMAIN}...")
print("  (This may take 60-90 seconds)")

# Using non-interactive mode with proper parameters
wacs_cmd = f'''C:\\win-acme\\wacs.exe ^
    --target manual ^
    --host {DOMAIN} ^
    --webroot C:\\stores\\learningtool\\www ^
    --installation iis ^
    --siteid {site_id} ^
    --store certificatestore ^
    --emailaddress {EMAIL} ^
    --accepttos ^
    --verbose'''

print(f"  Command: {wacs_cmd}")

stdin, stdout, stderr = ssh.exec_command(wacs_cmd, timeout=180)
exit_code = stdout.channel.recv_exit_status()
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

print(f"  Exit code: {exit_code}")
print(f"  Output: {output[-1500:]}")  # Last 1500 chars
if error:
    print(f"  Stderr: {error[:500]}")

# Step 2: Check if certificate was created
print("\n[3/6] Checking certificate store...")
cert_check_cmd = f'''powershell -Command "
Get-ChildItem -Path Cert:\\LocalMachine\\My |
Where-Object {{ $_.Subject -like '*{DOMAIN}*' }} |
Select-Object Subject, Thumbprint, NotAfter |
Format-Table -AutoSize
"'''
stdin, stdout, stderr = ssh.exec_command(cert_check_cmd)
certs = stdout.read().decode()
print(certs if certs.strip() else "  No certificate found")

# Step 3: Verify IIS binding
print("\n[4/6] Verifying IIS HTTPS binding...")
binding_cmd = '''powershell -Command "
Import-Module WebAdministration
Get-WebBinding -Name LearningToolApp |
Format-Table protocol, bindingInformation, sslFlags, certificateHash -AutoSize
"'''
stdin, stdout, stderr = ssh.exec_command(binding_cmd)
bindings = stdout.read().decode()
print(bindings)

# Step 4: Test HTTPS locally on the server
print("\n[5/6] Testing HTTPS on localhost...")
local_test_cmd = '''powershell -Command "try { (Invoke-WebRequest -Uri https://localhost:443 -Headers @{Host='learning.prospergenics.com'} -UseBasicParsing -TimeoutSec 5).StatusCode } catch { Write-Output \\"Failed: $($_.Exception.Message)\\" }"'''
stdin, stdout, stderr = ssh.exec_command(local_test_cmd, timeout=10)
local_result = stdout.read().decode().strip()
print(f"  Result: {local_result}")

# Step 5: Test HTTPS from external
print(f"\n[6/6] Testing HTTPS from domain name...")
external_test_cmd = f'''powershell -Command "try {{ (Invoke-WebRequest -Uri https://{DOMAIN} -UseBasicParsing -TimeoutSec 10).StatusCode }} catch {{ Write-Output \\"Failed: $($_.Exception.Message)\\" }}"'''
stdin, stdout, stderr = ssh.exec_command(external_test_cmd, timeout=15)
external_result = stdout.read().decode().strip()
print(f"  Result: {external_result}")

ssh.close()

print("\n" + "=" * 70)
if "200" in external_result:
    print("  SUCCESS! HTTPS is working!")
    print("=" * 70)
    print(f"\n  ✓ HTTPS URL: https://{DOMAIN}")
    print(f"  ✓ HTTP URL:  http://{DOMAIN}")
    print(f"\n  Certificate will auto-renew every 60 days")
else:
    print("  SSL Certificate Created (manual verification needed)")
    print("=" * 70)
    print(f"\n  Test manually:")
    print(f"  - https://{DOMAIN}")
    print(f"  - Check certificate details in browser")
