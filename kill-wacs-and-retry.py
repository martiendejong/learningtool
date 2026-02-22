#!/usr/bin/env python3
"""Kill existing Win-ACME process and retry SSL activation"""

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
print("  SSL Activation - Kill and Retry")
print("=" * 70)

# Kill existing wacs.exe processes
print("\n[1/5] Killing existing Win-ACME processes...")
kill_cmd = 'taskkill /F /IM wacs.exe 2>&1'
stdin, stdout, stderr = ssh.exec_command(kill_cmd)
kill_result = stdout.read().decode()
print(f"  {kill_result.strip()}")

time.sleep(2)

# Get IIS site ID
print("\n[2/5] Getting IIS site ID...")
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Import-Module WebAdministration; (Get-Website -Name LearningToolApp).ID"')
site_id = stdout.read().decode().strip()
print(f"  Site ID: {site_id}")

# Run Win-ACME
print(f"\n[3/5] Running Win-ACME for {DOMAIN}...")
wacs_cmd = f'C:\\win-acme\\wacs.exe --source manual --host {DOMAIN} --installation iissites --installationsiteid {site_id} --emailaddress {EMAIL} --accepttos --validation filesystem --validationsiteid {site_id} --webroot C:\\stores\\learningtool\\www'

stdin, stdout, stderr = ssh.exec_command(wacs_cmd, timeout=180)
exit_code = stdout.channel.recv_exit_status()
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

print(f"  Exit code: {exit_code}")
print(f"  Output (last 1000 chars):")
print(f"    {output[-1000:]}")

if error:
    print(f"  Errors: {error[:500]}")

# Check certificate
print("\n[4/5] Checking certificate...")
cert_cmd = f'powershell -Command "Get-ChildItem Cert:\\LocalMachine\\My | Where-Object {{ $_.Subject -like \'*{DOMAIN}*\' }} | Format-List Subject, Thumbprint, NotAfter"'
stdin, stdout, stderr = ssh.exec_command(cert_cmd)
cert_info = stdout.read().decode()

if DOMAIN in cert_info:
    print("  [OK] Certificate installed!")
    print(cert_info[:500])
else:
    print("  [WARN] Certificate not found in store")

# Test HTTPS
print(f"\n[5/5] Testing HTTPS...")
test_cmd = f'powershell -Command "try {{ (Invoke-WebRequest -Uri https://{DOMAIN} -UseBasicParsing -TimeoutSec 10).StatusCode }} catch {{ Write-Output \\"Error: $($_.Exception.Message)\\" }}"'
stdin, stdout, stderr = ssh.exec_command(test_cmd, timeout=15)
result = stdout.read().decode().strip()

print(f"  Result: {result}")

ssh.close()

print("\n" + "=" * 70)
if "200" in result:
    print("  SUCCESS! HTTPS IS ACTIVE!")
    print("=" * 70)
    print(f"\n  https://{DOMAIN}")
else:
    print("  Status Check Complete")
    print("=" * 70)
    print(f"\n  Manual verification: https://{DOMAIN}")
