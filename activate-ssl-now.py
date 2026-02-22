#!/usr/bin/env python3
"""Activate SSL certificate using Win-ACME"""

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
print("  Activating SSL Certificate with Win-ACME")
print("=" * 70)

# Get IIS site ID
print("\n[1/4] Getting IIS site ID...")
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Import-Module WebAdministration; (Get-Website -Name LearningToolApp).ID"')
site_id = stdout.read().decode().strip()
print(f"  Site ID: {site_id}")

# Run Win-ACME with simple parameters
print(f"\n[2/4] Running Win-ACME for {DOMAIN}...")
print("  This will take 30-60 seconds...")

# Simple command: Manual binding, IIS installation
wacs_cmd = f'''C:\\win-acme\\wacs.exe --source manual --host {DOMAIN} --installation iissites --installationsiteid {site_id} --emailaddress {EMAIL} --accepttos --validation filesystem --validationsiteid {site_id} --webroot C:\\stores\\learningtool\\www'''

print(f"  Command: {wacs_cmd}")
print()

stdin, stdout, stderr = ssh.exec_command(wacs_cmd, timeout=180)

# Read output line by line
print("  Output:")
for line in stdout:
    print(f"    {line.rstrip()}")

exit_code = stdout.channel.recv_exit_status()
print(f"\n  Exit code: {exit_code}")

# Check if certificate was installed
print("\n[3/4] Checking certificate installation...")
cert_cmd = f'''powershell -Command "Get-ChildItem Cert:\\LocalMachine\\My | Where-Object {{ $_.Subject -like '*{DOMAIN}*' }} | Select-Object Subject, Thumbprint, NotAfter | Format-List"'''
stdin, stdout, stderr = ssh.exec_command(cert_cmd)
cert_info = stdout.read().decode()

if cert_info.strip():
    print("  [OK] Certificate found in store:")
    print(cert_info)
else:
    print("  [WARN] No certificate found")

# Check HTTPS binding
print("\n[4/4] Checking HTTPS binding...")
binding_cmd = '''powershell -Command "Import-Module WebAdministration; Get-WebBinding -Name LearningToolApp -Protocol https | Select-Object protocol, bindingInformation, certificateHash | Format-List"'''
stdin, stdout, stderr = ssh.exec_command(binding_cmd)
binding_info = stdout.read().decode()

if binding_info.strip():
    print("  [OK] HTTPS binding:")
    print(binding_info)
else:
    print("  [WARN] No HTTPS binding found")

# Test HTTPS
print("\n[5/4] Testing HTTPS access...")
test_cmd = f'''powershell -Command "try {{ $response = Invoke-WebRequest -Uri https://{DOMAIN} -UseBasicParsing -TimeoutSec 10; Write-Output \\"Status: $($response.StatusCode)\\" }} catch {{ Write-Output \\"Failed: $($_.Exception.Message)\\" }}"'''
stdin, stdout, stderr = ssh.exec_command(test_cmd, timeout=15)
test_result = stdout.read().decode().strip()
print(f"  {test_result}")

ssh.close()

print("\n" + "=" * 70)
if "Status: 200" in test_result:
    print("  SUCCESS! HTTPS is now active!")
    print("=" * 70)
    print(f"\n  https://{DOMAIN}")
    print(f"\n  Certificate will auto-renew every 60 days")
else:
    print("  SSL Configuration Status")
    print("=" * 70)
    print("\n  Check the output above for details")
    print(f"  Manual verification: https://{DOMAIN}")
