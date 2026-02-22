#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Setting ASPNETCORE_ENVIRONMENT to Production ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
Import-Module WebAdministration
$appPoolName = 'LearningToolPool'

# Set environment variable for app pool
$appPool = Get-Item IIS:\AppPools\$appPoolName
Clear-ItemProperty IIS:\AppPools\$appPoolName -Name environmentVariables
New-ItemProperty IIS:\AppPools\$appPoolName -Name environmentVariables -Value @{name='ASPNETCORE_ENVIRONMENT';value='Production'}

Write-Host 'Environment variable set to Production'

# Verify
$vars = Get-ItemProperty IIS:\AppPools\$appPoolName -Name environmentVariables
Write-Host 'Current environment variables:'
$vars.environmentVariables | Format-Table
"''')
print(stdout.read().decode('utf-8', errors='ignore'))
err = stderr.read().decode('utf-8', errors='ignore')
if err:
    print('STDERR:', err)

print('\n=== Restarting App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
print('Restarted')

import time
time.sleep(4)

print('\n=== Testing ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')

if 'healthy' in response.lower() or (response and len(response) < 100 and '<!DOCTYPE' not in response):
    print('SUCCESS! API is working!')
    print(response)
else:
    print('Response:', response[:500] if response else 'Empty')

    # Check if migration is still being attempted
    stdin2, stdout2, stderr2 = ssh.exec_command(r'powershell -Command "Get-Content C:\stores\learningtool\backend\logs\*.log -Tail 20 | Select-String -Pattern \"Applying migration\""')
    migration_log = stdout2.read().decode('utf-8', errors='ignore')
    if migration_log.strip():
        print('\nWARNING: Still trying to apply migrations:')
        print(migration_log)
    else:
        print('\nGood: No migration attempts in latest log')

ssh.close()
