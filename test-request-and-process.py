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

print('=== Step 1: Check processes BEFORE request ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name dotnet,w3wp -ErrorAction SilentlyContinue | Format-Table Id,ProcessName,StartTime -AutoSize"')
before = stdout.read().decode('utf-8', errors='ignore')
print(before if before.strip() else '(No processes)')

print('\n=== Step 2: Make API request ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(f'Response: {response if response else "(empty)"}')

time.sleep(2)

print('\n=== Step 3: Check processes IMMEDIATELY after request ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name dotnet,w3wp -ErrorAction SilentlyContinue | Format-Table Id,ProcessName,StartTime,Path -AutoSize"')
after = stdout.read().decode('utf-8', errors='ignore')
print(after if after.strip() else '(No processes)')

time.sleep(5)

print('\n=== Step 4: Check processes 5 seconds later ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name dotnet,w3wp -ErrorAction SilentlyContinue | Format-Table Id,ProcessName,StartTime -AutoSize"')
later = stdout.read().decode('utf-8', errors='ignore')
print(later if later.strip() else '(No processes)')

print('\n=== Step 5: Check latest log to see what happened ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 20"')
log = stdout.read().decode('utf-8', errors='replace')
if log.strip():
    print(log)
else:
    print('(Log empty)')

ssh.close()
