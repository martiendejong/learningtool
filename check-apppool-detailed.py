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

print('=== App Pool Basic Info ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-IISAppPool -Name LearningToolPool"')
basic = stdout.read().decode('utf-8', errors='ignore')
print(basic)

print('\n=== App Pool .NET CLR Version ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "$pool = Get-IISAppPool -Name LearningToolPool; $pool.ManagedRuntimeVersion"')
clr = stdout.read().decode('utf-8', errors='ignore')
print(f'CLR Version: {clr}')

print('\n=== App Pool Pipeline Mode ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "$pool = Get-IISAppPool -Name LearningToolPool; $pool.ManagedPipelineMode"')
pipeline = stdout.read().decode('utf-8', errors='ignore')
print(f'Pipeline Mode: {pipeline}')

print('\n=== App Pool Start Mode ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "$pool = Get-IISAppPool -Name LearningToolPool; $pool.StartMode"')
startmode = stdout.read().decode('utf-8', errors='ignore')
print(f'Start Mode: {startmode}')

print('\n=== Current Running Processes ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name dotnet -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,StartTime,Path | Format-Table -AutoSize"')
procs = stdout.read().decode('utf-8', errors='ignore')
print(procs if procs.strip() else '(No dotnet processes running)')

ssh.close()
