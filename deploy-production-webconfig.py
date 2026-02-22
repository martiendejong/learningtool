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

print('=== Step 1: Upload web.config with ASPNETCORE_ENVIRONMENT=Production ===')
sftp = ssh.open_sftp()
sftp.put('C:\\Projects\\learningtool\\web.config.production', 'C:\\stores\\learningtool\\backend\\web.config')
sftp.close()
print('✓ Uploaded web.config with environment variable set to Production')

print('\n=== Step 2: Recycle app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
stdout.read()
print('✓ App pool recycled')

time.sleep(5)

print('\n=== Step 3: Test API health endpoint ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(f'API Response: {response if response else "(empty response)"}')

print('\n=== Step 4: Check latest log ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Select-Object Name,LastWriteTime,Length"')
latest = stdout.read().decode('utf-8', errors='ignore')
print(latest)

time.sleep(2)

print('\n=== Step 5: Show log content ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content"')
log_content = stdout.read().decode('utf-8', errors='replace')
if log_content.strip():
    print(log_content)
else:
    print('(Log is empty - app might not have started yet, waiting...)')
    time.sleep(3)
    stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content"')
    log_content = stdout.read().decode('utf-8', errors='replace')
    print(log_content if log_content.strip() else '(Still empty)')

print('\n=== Step 6: Test with full URL from browser ===')
stdin, stdout, stderr = ssh.exec_command('curl -v http://learning.prospergenics.com/api/auth/health 2>&1 | head -30')
browser_test = stdout.read().decode('utf-8', errors='ignore')
print(browser_test)

ssh.close()
