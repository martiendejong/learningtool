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

print('=== Check 1: All recent log files ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 5 | Format-Table Name,LastWriteTime,Length -AutoSize"')
logs = stdout.read().decode('utf-8', errors='ignore')
print(logs)

print('\n=== Check 2: Content of each recent log ===')
for i in range(1, 4):
    print(f'\n--- Log #{i} ---')
    stdin, stdout, stderr = ssh.exec_command(f'powershell -Command "Get-ChildItem C:\\stores\\learningtool\\backend\\logs | Sort-Object LastWriteTime -Descending | Select-Object -First {i} -Skip {i-1} | Get-Content"')
    content = stdout.read().decode('utf-8', errors='replace')
    if content.strip():
        print(content)
    else:
        print('(empty)')

print('\n=== Check 3: Windows Event Log - Recent ASP.NET Core errors ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \'IIS AspNetCore Module V2\' -Newest 5 | Format-List TimeGenerated,EntryType,Message"')
events = stdout.read().decode('utf-8', errors='replace')
print(events)

print('\n=== Check 4: App Pool Status ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebAppPoolState LearningToolPool"')
status = stdout.read().decode('utf-8', errors='ignore')
print(f'App Pool Status: {status}')

print('\n=== Check 5: Verify web.config was uploaded correctly ===')
stdin, stdout, stderr = ssh.exec_command('type C:\\stores\\learningtool\\backend\\web.config')
webconfig = stdout.read().decode('utf-8', errors='replace')
print(webconfig)

print('\n=== Check 6: Trigger another request and wait for logs ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(f'Response: {response if response else "(empty)"}')

time.sleep(5)

print('\n=== Check 7: Read latest log again after waiting ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content"')
final_log = stdout.read().decode('utf-8', errors='replace')
if final_log.strip():
    print(final_log)
else:
    print('(still empty)')

ssh.close()
