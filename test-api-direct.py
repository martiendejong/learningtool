#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Test API endpoints various ways ===')

# Try the health endpoint with full path
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
print('1. localhost/api/auth/health:', stdout.read().decode('utf-8', errors='ignore') or 'Empty')

# Try without /auth
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/health')
print('2. localhost/api/health:', stdout.read().decode('utf-8', errors='ignore') or 'Empty')

# Try just /api
stdin, stdout, stderr = ssh.exec_command('curl -s -w "\\nHTTP: %{http_code}" -H "Host: learning.prospergenics.com" http://localhost/api/')
print('3. localhost/api/:', stdout.read().decode('utf-8', errors='ignore'))

# Try on port 5192 directly
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
print('4. localhost:5192/api/auth/health:', stdout.read().decode('utf-8', errors='ignore') or 'Empty')

# Check if site is even running
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebAppPoolState LearningToolPool"')
print('\\n5. App Pool State:', stdout.read().decode('utf-8', errors='ignore'))

# Check for recent startup errors
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -EntryType Error -After (Get-Date).AddMinutes(-2) -Source \\"IIS AspNetCore Module V2\\" -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count"')
error_count = stdout.read().decode('utf-8', errors='ignore').strip()
print(f'6. Errors in last 2 minutes: {error_count}')

ssh.close()
