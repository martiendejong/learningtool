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

print('=== ASP.NET Core Module errors in last hour ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \'IIS AspNetCore Module V2\' -After (Get-Date).AddHours(-1) -ErrorAction SilentlyContinue | Format-List TimeGenerated,EntryType,Message"')
events = stdout.read().decode('utf-8', errors='replace')
print(events if events.strip() else '(No events)')

print('\n\n=== .NET Runtime errors in last hour ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \'.NET Runtime\' -After (Get-Date).AddHours(-1) -ErrorAction SilentlyContinue | Format-List TimeGenerated,EntryType,Message"')
dotnet_events = stdout.read().decode('utf-8', errors='replace')
print(dotnet_events if dotnet_events.strip() else '(No events)')

print('\n\n=== Application errors in last 30 minutes ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -EntryType Error -After (Get-Date).AddMinutes(-30) -ErrorAction SilentlyContinue | Select-Object -First 5 | Format-List TimeGenerated,Source,Message"')
app_errors = stdout.read().decode('utf-8', errors='replace')
print(app_errors if app_errors.strip() else '(No errors)')

ssh.close()
