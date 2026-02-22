#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import paramiko
import sys

# Set UTF-8 output
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Uploading updated appsettings.Production.json ===')
sftp = ssh.open_sftp()
sftp.put(r'C:\Projects\learningtool\src\LearningTool.API\appsettings.Production.json',
         r'C:\stores\learningtool\backend\appsettings.Production.json')
sftp.close()
print('✓ Config file uploaded')

print('\n=== Recycling IIS app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningTool"')
result = stdout.read().decode('utf-8', errors='replace')
error = stderr.read().decode('utf-8', errors='replace')
if error:
    print(f'Error: {error}')
else:
    print('✓ App pool recycled')

print('\n=== Waiting 3 seconds for app to restart ===')
import time
time.sleep(3)

print('\n=== Checking if process is running ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name dotnet -ErrorAction SilentlyContinue | Format-Table Id,ProcessName,StartTime -AutoSize"')
processes = stdout.read().decode('utf-8', errors='ignore')
print(processes if processes.strip() else '(No dotnet processes)')

ssh.close()
print('\n✓ Deployment complete')
