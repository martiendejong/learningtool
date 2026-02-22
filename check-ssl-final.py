#!/usr/bin/env python3
"""Final SSL status check"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"
DOMAIN = "learning.prospergenics.com"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=" * 70)
print("  SSL Status Check")
print("=" * 70)

# Check 1: Certificate in store
print("\n[1/4] Checking certificate in store...")
cert_cmd = f'powershell -Command "Get-ChildItem Cert:\\LocalMachine\\My | Where-Object {{ $_.Subject -like \'*{DOMAIN}*\' }} | Select-Object Subject, Thumbprint, NotAfter | Format-List"'
stdin, stdout, stderr = ssh.exec_command(cert_cmd)
cert_info = stdout.read().decode()

if DOMAIN in cert_info:
    print("  [OK] Certificate found!")
    for line in cert_info.split('\n')[:5]:
        if line.strip():
            print(f"    {line.strip()}")
else:
    print("  [WARN] No certificate found")

# Check 2: HTTPS binding
print("\n[2/4] Checking HTTPS binding...")
binding_cmd = 'powershell -Command "Import-Module WebAdministration; Get-WebBinding -Name LearningToolApp -Protocol https | Select-Object protocol, bindingInformation, certificateHash | Format-List"'
stdin, stdout, stderr = ssh.exec_command(binding_cmd)
binding_info = stdout.read().decode()

if binding_info.strip() and "443" in binding_info:
    print("  [OK] HTTPS binding configured")
    for line in binding_info.split('\n')[:4]:
        if line.strip():
            print(f"    {line.strip()}")
else:
    print("  [WARN] No HTTPS binding")

# Check 3: Test HTTPS locally
print(f"\n[3/4] Testing HTTPS on localhost...")
local_test = 'powershell -Command "try { (Invoke-WebRequest -Uri https://localhost:443 -Headers @{Host=\'learning.prospergenics.com\'} -UseBasicParsing -TimeoutSec 5).StatusCode } catch { Write-Output $_.Exception.Message }"'
stdin, stdout, stderr = ssh.exec_command(local_test, timeout=10)
local_result = stdout.read().decode().strip()
print(f"  Result: {local_result}")

# Check 4: Test HTTPS externally
print(f"\n[4/4] Testing HTTPS from domain...")
external_test = f'powershell -Command "try {{ (Invoke-WebRequest -Uri https://{DOMAIN} -UseBasicParsing -TimeoutSec 10).StatusCode }} catch {{ Write-Output $_.Exception.Message }}"'
stdin, stdout, stderr = ssh.exec_command(external_test, timeout=15)
external_result = stdout.read().decode().strip()
print(f"  Result: {external_result}")

ssh.close()

print("\n" + "=" * 70)
if "200" in external_result:
    print("  SUCCESS! HTTPS IS ACTIVE!")
    print("=" * 70)
    print(f"\n  https://{DOMAIN}")
    print(f"\n  Certificate auto-renews every 60 days")
elif "200" in local_result:
    print("  PARTIAL SUCCESS")
    print("=" * 70)
    print("\n  Certificate installed but external access failing")
    print("  Possible causes:")
    print("  - DNS not propagated")
    print("  - Firewall blocking port 443")
    print("  - Certificate not bound correctly")
else:
    print("  SSL NOT YET ACTIVE")
    print("=" * 70)
    print("\n  Win-ACME may still be running")
    print("  Check task status or wait a few more minutes")
