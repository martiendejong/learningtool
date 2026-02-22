#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Fixing Database Permissions ===')

# Grant full control to IIS AppPool\LearningToolPool on database directory
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data /grant "IIS AppPool\LearningToolPool:(OI)(CI)F" /T')
print(stdout.read().decode('utf-8', errors='ignore'))
err = stderr.read().decode('utf-8', errors='ignore')
if err:
    print('ERROR:', err)

print('\n=== Verifying Permissions ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Restarting App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
print('App pool restarted')

# Wait a moment
import time
time.sleep(3)

print('\n=== Checking App Pool Status ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebAppPoolState LearningToolPool"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Testing API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
