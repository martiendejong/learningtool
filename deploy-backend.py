#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import paramiko
import sys
import time
import os

# Set UTF-8 output
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Stopping IIS app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Stop-WebAppPool -Name LearningToolPool"')
stdout.read()
print('✓ App pool stopped')

print('\n=== Uploading backend files ===')
sftp = ssh.open_sftp()

# Upload DLL files
publish_dir = r'C:\Projects\learningtool\publish'
for file in os.listdir(publish_dir):
    if file.endswith('.dll') or file.endswith('.exe') or file.endswith('.json') or file.endswith('.pdb'):
        local_file = os.path.join(publish_dir, file)
        remote_file = f'C:\stores\learningtool\backend\{file}'
        try:
            sftp.put(local_file, remote_file)
            print(f'  Uploaded: {file}')
        except Exception as e:
            print(f'  Failed to upload {file}: {e}')

sftp.close()
print('✓ Files uploaded')

print('\n=== Starting IIS app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Start-WebAppPool -Name LearningToolPool"')
stdout.read()
print('✓ App pool started')

print('\n=== Waiting 3 seconds for app to start ===')
time.sleep(3)

print('\n=== Checking dotnet process ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name dotnet -ErrorAction SilentlyContinue | Format-Table Id,ProcessName,StartTime -AutoSize"')
processes = stdout.read().decode('utf-8', errors='ignore')
print(processes if processes.strip() else '(No dotnet processes - will start on first request)')

ssh.close()
print('\n✓ Backend deployment complete')
