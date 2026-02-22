#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Check DataProtection keys directory ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\backend\\DataProtection-Keys 2>&1')
result = stdout.read().decode('utf-8', errors='ignore')
print(result)

print('\n=== Create DataProtection directory if needed ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "New-Item -ItemType Directory -Path C:\\stores\\learningtool\\backend\\DataProtection-Keys -Force | Out-Null; Write-Host \'Directory created\'"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Grant permissions to DataProtection directory ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\backend\DataProtection-Keys /grant "IIS AppPool\LearningToolPool:(OI)(CI)(F)" /T')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Check Application event log for full DataProtection error ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Newest 10 | Where-Object { $_.Message -like \'*DataProtection*\' } | Select-Object -First 1 | Format-List TimeGenerated,EntryType,Message"')
error = stdout.read().decode('utf-8', errors='ignore')
if error.strip():
    print(error)
else:
    print('No DataProtection errors found')

print('\n=== Restart app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
stdout.read()
print('Restarted')

import time
time.sleep(5)

print('\n=== Test API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -k https://learning.prospergenics.com/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')

if 'healthy' in response.lower():
    print('SUCCESS! API is working!')
    print(response)
else:
    print('Response:', response[:300] if response else 'Empty')

ssh.close()
