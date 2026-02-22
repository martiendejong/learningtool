#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. List recent log files ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\stores\learningtool\backend\logs\*.log /od /tc | find "2026"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 2. Read newest log ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Content (Get-ChildItem C:\stores\learningtool\backend\logs\*.log | Sort LastWriteTime -Descending | Select -First 1).FullName -Tail 40"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 3. Test API endpoint with detailed output ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -w "\nSTATUS: %{http_code}\nTIME: %{time_total}s" https://learning.prospergenics.com/api/auth/health')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 4. Check if backend process is running ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process | Where-Object { $_.ProcessName -like \'*LearningTool*\' }"')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
