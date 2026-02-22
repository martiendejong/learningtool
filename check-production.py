#!/usr/bin/env python3
import paramiko
import sys

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())

try:
    ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

    # Check production config
    stdin, stdout, stderr = ssh.exec_command(r'type C:\stores\learningtool\backend\appsettings.Production.json')
    print('=== Production Config ===')
    config = stdout.read().decode('utf-8', errors='ignore')
    print(config)

    # Check if OpenAI key exists
    if 'ApiKey' in config:
        print('\n✓ OpenAI configuration found')
    else:
        print('\n✗ OpenAI configuration MISSING!')

    # Check backend process
    stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name LearningTool.API -ErrorAction SilentlyContinue | Select-Object ProcessName,Id"')
    print('\n=== Backend Process ===')
    print(stdout.read().decode('utf-8', errors='ignore'))

    # Check IIS app pool
    stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebAppPoolState LearningToolPool"')
    print('\n=== App Pool Status ===')
    print(stdout.read().decode('utf-8', errors='ignore'))

    # Check recent errors
    stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source ASP.NET* -Newest 5 -ErrorAction SilentlyContinue | Select-Object TimeGenerated,Message"')
    print('\n=== Recent Errors ===')
    print(stdout.read().decode('utf-8', errors='ignore'))

    ssh.close()
except Exception as e:
    print(f"Error: {e}")
    sys.exit(1)
