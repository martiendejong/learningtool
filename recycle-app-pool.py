#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import paramiko
import sys
import time

# Set UTF-8 output
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Recycling LearningToolPool app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
result = stdout.read().decode('utf-8', errors='replace')
error = stderr.read().decode('utf-8', errors='replace')
if error:
    print(f'Error: {error}')
else:
    print('✓ App pool recycled successfully')

print('\n=== Waiting 3 seconds for app to restart ===')
time.sleep(3)

print('\n=== Checking new dotnet process ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name dotnet -ErrorAction SilentlyContinue | Format-Table Id,ProcessName,StartTime -AutoSize"')
processes = stdout.read().decode('utf-8', errors='ignore')
print(processes if processes.strip() else '(No dotnet processes)')

ssh.close()
print('\n✓ Application restarted with new OpenAI API key')
