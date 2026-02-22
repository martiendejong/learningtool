#!/usr/bin/env python3
"""Check if Certbot was installed via WinGet"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("Checking WinGet Certbot installation status...")
stdin, stdout, stderr = ssh.exec_command('winget list Certbot', timeout=30)
output = stdout.read().decode()

print(output)

# If installed, try to find the executable
if "Certbot" in output:
    print("\nSearching for certbot.exe...")
    cmd = r'dir /s /b "C:\Program Files\WindowsApps\*certbot*.exe" 2>nul'
    stdin, stdout, stderr = ssh.exec_command(cmd, timeout=30)
    paths = stdout.read().decode().strip()

    if paths:
        print("Found:")
        for path in paths.split('\n'):
            if path.strip():
                print(f"  {path.strip()}")
    else:
        print("  Not found in WindowsApps")

ssh.close()
