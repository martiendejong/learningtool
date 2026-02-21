#!/usr/bin/env python3
"""Add domain binding for learning.prospergenics.com"""

import paramiko

SSH_HOST = "85.215.217.154"
SSH_USER = "administrator"
SSH_PASS = "3WsXcFr$7YhNmKi*"

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(SSH_HOST, username=SSH_USER, password=SSH_PASS)

print("Adding domain binding for learning.prospergenics.com...")

# Add binding for port 80 with hostname
cmd = 'powershell -Command "Import-Module WebAdministration; New-WebBinding -Name LearningToolApp -Protocol http -Port 80 -HostHeader learning.prospergenics.com"'
stdin, stdout, stderr = ssh.exec_command(cmd)
output = stdout.read().decode('utf-8', errors='ignore')
error = stderr.read().decode('utf-8', errors='ignore')

if output.strip():
    print(output)
if error.strip():
    print(f"Error: {error}")

# Verify bindings
print("\nVerifying bindings...")
cmd2 = 'powershell -Command "Import-Module WebAdministration; Get-WebBinding -Name LearningToolApp"'
stdin, stdout, stderr = ssh.exec_command(cmd2)
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()

print("\nDone! Test at: http://learning.prospergenics.com")
