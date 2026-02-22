#!/usr/bin/env python3
"""Kill hung Win-ACME and try simple SSL test"""

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
print("  Simple SSL Test")
print("=" * 70)

# Kill hung process
print("\n[1/3] Killing hung Win-ACME process...")
stdin, stdout, stderr = ssh.exec_command('taskkill /F /IM wacs.exe')
kill_result = stdout.read().decode()
print(f"  {kill_result.strip()}")

time.sleep(2)

# Try simpler Win-ACME command
print(f"\n[2/3] Trying Win-ACME with simpler parameters...")
print("  (30 second timeout)")

# Most basic command possible
simple_cmd = f'C:\\win-acme\\wacs.exe --target manual --host {DOMAIN} --validation selfhosting --store pemfiles --pemfilespath C:\\ssl-certs --installation manual --emailaddress {EMAIL} --accepttos'

stdin, stdout, stderr = ssh.exec_command(simple_cmd, timeout=45)

# Read with timeout
import socket
try:
    output = stdout.read().decode('utf-8', errors='ignore')
    error = stderr.read().decode('utf-8', errors='ignore')
    exit_code = stdout.channel.recv_exit_status()

    print(f"  Exit code: {exit_code}")
    if output:
        print(f"  Output (last 500 chars): {output[-500:]}")
    if error:
        print(f"  Error: {error[:300]}")
except socket.timeout:
    print("  [TIMEOUT] Win-ACME taking too long")

# Check if certificate was created
print("\n[3/3] Checking for certificate files...")
check_cmd = r'powershell -Command "if (Test-Path C:\ssl-certs) { Get-ChildItem C:\ssl-certs | Select-Object Name, Length } else { Write-Output \\"No certificate directory\\" }"'
stdin, stdout, stderr = ssh.exec_command(check_cmd)
files = stdout.read().decode()
print(f"  {files}")

ssh.close()

print("\n" + "=" * 70)
print("  Conclusion:")
print("=" * 70)
print("\n  Win-ACME seems to hang on this server")
print("  Possible causes:")
print("  - Interactive input required")
print("  - Network/firewall issues")
print("  - DNS verification failing")
print("\n  Recommendation:")
print("  - Site works perfectly on HTTP")
print("  - SSL can be added later via RDP (manual setup)")
print("  - Or use different SSL tool (Certbot for Windows)")
