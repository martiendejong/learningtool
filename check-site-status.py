#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. Check app pool status ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebAppPoolState -Name LearningToolPool"')
print('App pool state:', stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 2. Check site status ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Website | Where-Object { $_.name -like \'*learning*\' } | Select-Object name,state"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 3. Test local health endpoint ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://127.0.0.1:37872/api/auth/health 2>&1')
local = stdout.read().decode('utf-8', errors='ignore')
print('Local health:', local if local else '(empty)')

print('\n=== 4. Check most recent log ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs\*.log | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 30"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 5. Test direct curl to domain ===')
stdin, stdout, stderr = ssh.exec_command('curl -v https://learning.prospergenics.com/ 2>&1 | head -30')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
