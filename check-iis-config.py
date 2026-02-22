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

print('=== Listing all IIS websites ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Website | Format-Table Name,State,PhysicalPath,Bindings -AutoSize"')
sites = stdout.read().decode('utf-8', errors='replace')
print(sites)

print('\n=== Listing all IIS App Pools ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-IISAppPool | Format-Table Name,State -AutoSize"')
pools = stdout.read().decode('utf-8', errors='replace')
print(pools)

print('\n=== Checking dotnet processes ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name dotnet -ErrorAction SilentlyContinue | Format-Table Id,ProcessName,StartTime,Path -AutoSize"')
processes = stdout.read().decode('utf-8', errors='ignore')
print(processes if processes.strip() else '(No dotnet processes)')

ssh.close()
