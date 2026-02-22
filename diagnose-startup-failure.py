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

print('=== Check 1: Does LearningTool.API.dll exist? ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Test-Path C:\\stores\\learningtool\\backend\\LearningTool.API.dll"')
dll_exists = stdout.read().decode('utf-8', errors='ignore').strip()
print(f'DLL exists: {dll_exists}')

if dll_exists == 'True':
    print('\n=== Check 2: DLL file size and date ===')
    stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Item C:\\stores\\learningtool\\backend\\LearningTool.API.dll | Select-Object Length,LastWriteTime"')
    dll_info = stdout.read().decode('utf-8', errors='ignore')
    print(dll_info)

print('\n=== Check 3: .NET Runtime version ===')
stdin, stdout, stderr = ssh.exec_command('dotnet --version')
dotnet_version = stdout.read().decode('utf-8', errors='ignore')
print(f'Installed: {dotnet_version}')

print('\n=== Check 4: Check Windows Event Log for ASP.NET Core errors ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \'IIS AspNetCore Module V2\' -Newest 3 | Format-List TimeGenerated,EntryType,Message"')
event_log = stdout.read().decode('utf-8', errors='replace')
print(event_log)

print('\n=== Check 5: List all files in backend directory ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-ChildItem C:\\stores\\learningtool\\backend | Select-Object Name,Length | Format-Table -AutoSize"')
files = stdout.read().decode('utf-8', errors='ignore')
print(files)

print('\n=== Check 6: Check appsettings.json exists ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Test-Path C:\\stores\\learningtool\\backend\\appsettings.json"')
appsettings_exists = stdout.read().decode('utf-8', errors='ignore').strip()
print(f'appsettings.json exists: {appsettings_exists}')

ssh.close()
