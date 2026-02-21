#!/usr/bin/env python3
"""Verify deployment status"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("=== Checking Files ===")
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Test-Path C:\\stores\\learningtool\\www\\index.html"')
print(f"Frontend: {stdout.read().decode().strip()}")

stdin, stdout, stderr = ssh.exec_command('powershell -Command "Test-Path C:\\stores\\learningtool\\backend\\LearningTool.API.dll"')
print(f"Backend: {stdout.read().decode().strip()}")

print("\n=== Checking IIS ===")
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Import-Module WebAdministration; Get-Website | Select-Object Name, State | Format-Table -AutoSize"')
websites = stdout.read().decode()
errors = stderr.read().decode()
print(websites if websites.strip() else "No output")
if errors.strip():
    print(f"Errors: {errors}")

print("\n=== Checking Port ===")
stdin, stdout, stderr = ssh.exec_command('powershell -Command "netstat -an | Select-String \':5192\'"')
port = stdout.read().decode()
print(port if port.strip() else "Port 5192 not listening")

ssh.close()
