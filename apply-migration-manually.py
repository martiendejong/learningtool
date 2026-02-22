#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Check what migrations are pending
print('=== Checking migrations ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
cd C:\stores\learningtool\backend
dotnet ef migrations list --no-build 2>&1
"''')
print(stdout.read().decode('utf-8', errors='ignore'))

# Apply pending migrations
print('\n=== Applying migrations ===')
stdin, stdout, stderr = ssh.exec_command(r'''powershell -Command "
cd C:\stores\learningtool\backend
dotnet ef database update --no-build 2>&1
"''')
output = stdout.read().decode('utf-8', errors='ignore')
print(output)

print('\n=== Restart App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
print('Restarted')

import time
time.sleep(3)

print('\n=== Test API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
if 'error' in response.lower() or '<!DOCTYPE' in response:
    print('Still getting error')
    print(response[:500])
else:
    print('SUCCESS!')
    print(response)

ssh.close()
