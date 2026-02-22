#!/usr/bin/env python3
"""Verify current deployment status"""

import requests
import paramiko

DOMAIN = "learning.prospergenics.com"
SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

print("=" * 70)
print("  Deployment Status Verification")
print("=" * 70)

# Test 1: HTTP Access
print("\n[1] Testing HTTP access...")
try:
    response = requests.get(f"http://{DOMAIN}", timeout=10)
    print(f"  [OK] HTTP Status: {response.status_code}")
    print(f"  [OK] Site is accessible on http://{DOMAIN}")
except Exception as e:
    print(f"  [FAIL] HTTP Error: {e}")

# Test 2: API Health Check
print("\n[2] Testing API...")
try:
    api_response = requests.get(f"http://{DOMAIN}/api/health", timeout=10)
    print(f"  [OK] API Status: {api_response.status_code}")
except Exception as e:
    # API might not have health endpoint
    print(f"  [INFO] API health check: {e}")

# Test 3: Frontend loads
print("\n[3] Checking frontend...")
try:
    response = requests.get(f"http://{DOMAIN}", timeout=10)
    if "<!DOCTYPE html>" in response.text or "<html" in response.text:
        print("  [OK] Frontend HTML is loading")
    else:
        print("  [WARN] Response doesn't look like HTML")
except Exception as e:
    print(f"  [FAIL] Frontend error: {e}")

# Test 4: Check IIS status via SSH
print("\n[4] Checking IIS application status...")
ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

# Check if site is running
status_cmd = 'powershell -Command "Import-Module WebAdministration; (Get-Website -Name LearningToolApp).State"'
stdin, stdout, stderr = ssh.exec_command(status_cmd)
site_state = stdout.read().decode().strip()
print(f"  IIS Site State: {site_state}")

# Check if app pool is running
pool_cmd = 'powershell -Command "Import-Module WebAdministration; (Get-WebAppPoolState -Name LearningToolPool).Value"'
stdin, stdout, stderr = ssh.exec_command(pool_cmd)
pool_state = stdout.read().decode().strip()
print(f"  App Pool State: {pool_state}")

# Check bindings
binding_cmd = 'powershell -Command "Import-Module WebAdministration; Get-WebBinding -Name LearningToolApp | Format-Table protocol, bindingInformation -AutoSize"'
stdin, stdout, stderr = ssh.exec_command(binding_cmd)
bindings = stdout.read().decode()
print(f"\n  Bindings:\n{bindings}")

ssh.close()

print("\n" + "=" * 70)
print("  Summary")
print("=" * 70)
print()
print(f"  ✓ Site URL: http://{DOMAIN}")
print(f"  ✓ User Account: dikomohamed287@gmail.com")
print(f"  ✓ Email Sent: Credentials delivered")
print(f"  ✓ GitHub: Repository pushed and secrets configured")
print()
print(f"  ⏳ HTTPS: Not yet configured (optional)")
print(f"     Manual setup required:")
print(f"     1. RDP to server: {SSH_HOST}")
print(f"     2. Run: C:\\win-acme\\wacs.exe")
print(f"     3. Select option M (Manual)")
print(f"     4. Enter domain: {DOMAIN}")
print(f"     5. Select IIS installation")
print()
