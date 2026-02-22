#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Testing if App Pool can write to directory ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
# Test write permissions by creating a file as the app pool user
$appPool = 'LearningToolPool'
$testFile = 'C:\stores\learningtool\data\test-write.txt'

# Use PowerShell to impersonate the app pool identity
Import-Module WebAdministration
$pool = Get-Item IIS:\AppPools\$appPool
Write-Host 'App Pool Identity:' $pool.processModel.identityType $pool.processModel.userName

# Try to create a test file
try {
    'test' | Out-File $testFile -Force
    Write-Host 'Write test SUCCESS'
    Remove-Item $testFile
} catch {
    Write-Host 'Write test FAILED:' $_.Exception.Message
}
"''')
print(stdout.read().decode('utf-8', errors='ignore'))
err_output = stderr.read().decode('utf-8', errors='ignore')
if err_output:
    print('STDERR:', err_output)

print('\n=== Grant MODIFY rights to entire directory ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data /grant "IIS AppPool\LearningToolPool:(OI)(CI)(M)" /T')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Reset Inheritance (make sure not blocked) ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data /reset /T')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Re-grant Full Control ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data /grant:r "IIS AppPool\LearningToolPool:(OI)(CI)(F)" /T')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Current Permissions ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Restart App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
print('Restarted')

import time
time.sleep(4)

print('\n=== Check for errors in Event Log ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \\"IIS AspNetCore Module V2\\" -Newest 1 | Select-Object -ExpandProperty Message"')
msg = stdout.read().decode('utf-8', errors='ignore')
if 'readonly database' in msg:
    print('STILL HAS READONLY ERROR')
elif 'exception' in msg.lower():
    print('Different error now:')
    print(msg[:1000])
else:
    print('No recent errors - might be working!')

print('\n=== Test API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
if response and 'healthy' in response.lower():
    print('SUCCESS!')
elif '<!DOCTYPE' in response:
    print('Still getting HTML error page')
else:
    print('Response:', response[:300] if response else 'No response')

ssh.close()
