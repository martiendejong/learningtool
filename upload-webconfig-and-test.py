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

print('=== Step 1: Upload new web.config with logging enabled ===')
sftp = ssh.open_sftp()
sftp.put('C:\\Projects\\learningtool\\web.config.new', 'C:\\stores\\learningtool\\backend\\web.config')
sftp.close()
print('✓ Uploaded web.config with stdoutLogEnabled="true"')

print('\n=== Step 2: Recycle app pool to force reload ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
stdout.read()
print('✓ App pool recycled')

time.sleep(3)

print('\n=== Step 3: Trigger API request to generate logs ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(f'Response: {response if response else "Empty"}')

time.sleep(2)

print('\n=== Step 4: Check for NEW log files ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 3 | Format-Table Name,LastWriteTime -AutoSize"')
logs = stdout.read().decode('utf-8', errors='ignore')
print(logs)

print('\n=== Step 5: Show content of latest log ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 50"')
log_content = stdout.read().decode('utf-8', errors='replace')
print(log_content)

ssh.close()
