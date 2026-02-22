#!/usr/bin/env python3
"""Check Win-ACME process and logs"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("Checking Win-ACME status...")
print()

# Check if process running
stdin, stdout, stderr = ssh.exec_command('tasklist | findstr wacs.exe')
processes = stdout.read().decode()

if processes.strip():
    print("[INFO] Win-ACME still running:")
    print(processes)
else:
    print("[INFO] No Win-ACME processes running")

# Check logs
print("\nChecking Win-ACME logs...")
log_cmd = r'powershell -Command "Get-ChildItem C:\ProgramData\win-acme\*\Log -Filter *.txt | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 40"'
stdin, stdout, stderr = ssh.exec_command(log_cmd, timeout=30)
logs = stdout.read().decode()

if logs.strip():
    print("Recent log entries:")
    print(logs)
else:
    print("[WARN] No logs found")

ssh.close()
