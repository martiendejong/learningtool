#!/usr/bin/env python3
import paramiko
import time

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Get current log file timestamp ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Select-Object Name,LastWriteTime"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Triggering API request ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print('Response:', response if response else 'Empty')

time.sleep(2)

print('\n=== Check log file again ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Select-Object Name,LastWriteTime"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Last 10 lines of latest log ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 10"')
log = stdout.read().decode('utf-8', errors='replace')
print(log)

print('\n=== Check if web.config exists and is valid ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Test-Path C:\\stores\\learningtool\\backend\\web.config"')
exists = stdout.read().decode('utf-8', errors='ignore').strip()
print('web.config exists:', exists)

ssh.close()
