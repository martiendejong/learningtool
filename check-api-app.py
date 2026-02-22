#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== IIS Application Configuration ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebApplication -Site LearningToolApp -Name api | Format-List *"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Check if backend directory has files ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\backend\\*.dll')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Check if web.config exists ===')
stdin, stdout, stderr = ssh.exec_command('type C:\\stores\\learningtool\\backend\\web.config')
print(stdout.read().decode('utf-8', errors='ignore')[:1000])

print('\n=== App Pool State ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebAppPoolState LearningToolPool"')
print(stdout.read().decode('utf-8', errors='ignore'))

# Test locally on the server
print('\n=== Test API locally on server ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost/api/auth/health')
local_response = stdout.read().decode('utf-8', errors='ignore')
print('Response:', local_response[:500] if local_response else 'Empty')

ssh.close()
