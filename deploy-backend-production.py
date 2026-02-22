#!/usr/bin/env python3
import paramiko
import os

# Files to deploy
dll_files = [
    ('src/LearningTool.API/bin/Release/net8.0/LearningTool.API.dll', 'LearningTool.API.dll'),
    ('src/LearningTool.Application/bin/Release/net8.0/LearningTool.Application.dll', 'LearningTool.Application.dll'),
    ('src/LearningTool.Domain/bin/Release/net8.0/LearningTool.Domain.dll', 'LearningTool.Domain.dll'),
    ('src/LearningTool.Infrastructure/bin/Release/net8.0/LearningTool.Infrastructure.dll', 'LearningTool.Infrastructure.dll'),
]

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. Stop app pool ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Stop-WebAppPool -Name LearningToolPool"')
stdout.read()
print('Stopped')

import time
time.sleep(3)

print('\n=== 2. Upload DLL files ===')
sftp = ssh.open_sftp()
for local_path, remote_name in dll_files:
    local_full = f'C:/Projects/learningtool/{local_path}'
    remote_full = f'C:/stores/learningtool/backend/{remote_name}'
    
    if os.path.exists(local_full):
        print(f'Uploading {remote_name}...')
        sftp.put(local_full, remote_full)
        print(f'  OK ({os.path.getsize(local_full)} bytes)')
    else:
        print(f'  SKIP (not found: {local_full})')

sftp.close()
print('Upload complete')

print('\n=== 3. Start app pool ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Start-WebAppPool -Name LearningToolPool"')
stdout.read()
print('Started')

time.sleep(8)

print('\n=== 4. Check logs ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "$f = Get-ChildItem C:\stores\learningtool\backend\logs\stdout*.log | Sort LastWriteTime -Desc | Select -First 1; Get-Content $f -Tail 15"')
log = stdout.read().decode('utf-8', errors='ignore')
print('Latest log:')
print(log)

print('\n=== 5. Test API ===')
stdin, stdout, stderr = ssh.exec_command(r'curl -s -k https://learning.prospergenics.com/api/auth/health')
health = stdout.read().decode('utf-8', errors='ignore')
print('Health check:', repr(health))

if health:
    print('\n✓ Deployment successful!')
else:
    print('\n⚠ API not responding yet - check logs')

ssh.close()
