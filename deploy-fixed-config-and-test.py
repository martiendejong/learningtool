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

print('=== Step 1: Upload fixed appsettings.Production.json (removed Kestrel config) ===')
sftp = ssh.open_sftp()
sftp.put('C:\\Projects\\learningtool\\appsettings.Production.json.fixed', 'C:\\stores\\learningtool\\backend\\appsettings.Production.json')
sftp.close()
print('✓ Uploaded appsettings.Production.json without Kestrel configuration')

print('\n=== Step 2: Recycle app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
stdout.read()
print('✓ App pool recycled')

time.sleep(3)

print('\n=== Step 3: Test API health endpoint ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(f'Response: {response if response else "Empty (checking if this is fixed...)"}')

time.sleep(2)

print('\n=== Step 4: Check latest log files ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 3 | Format-Table Name,LastWriteTime,Length -AutoSize"')
logs = stdout.read().decode('utf-8', errors='ignore')
print(logs)

print('\n=== Step 5: Show latest log content ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content"')
log_content = stdout.read().decode('utf-8', errors='replace')
if log_content.strip():
    print(log_content)
else:
    print('(Log file is empty or application hasn\'t logged anything yet)')

print('\n=== Step 6: Test API endpoint with verbose output ===')
stdin, stdout, stderr = ssh.exec_command('curl -v -H "Host: learning.prospergenics.com" http://localhost/api/auth/health 2>&1')
verbose_response = stdout.read().decode('utf-8', errors='ignore')
print(verbose_response)

ssh.close()
