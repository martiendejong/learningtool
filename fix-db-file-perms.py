#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Checking Current File Permissions ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data\learningtool.db')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Removing Readonly Attribute ===')
stdin, stdout, stderr = ssh.exec_command(r'attrib -R C:\stores\learningtool\data\learningtool.db')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Grant Full Control to App Pool (direct on file) ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data\learningtool.db /grant "IIS AppPool\LearningToolPool:F" /T')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Also check for WAL and SHM files ===')
stdin, stdout, stderr = ssh.exec_command(r'dir C:\stores\learningtool\data\*.db*')
files_output = stdout.read().decode('utf-8', errors='ignore')
print(files_output)

# Grant permissions to all db-related files
if 'db-wal' in files_output or 'db-shm' in files_output:
    print('\n=== Fixing WAL/SHM permissions ===')
    stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data\*.* /grant "IIS AppPool\LearningToolPool:F"')
    print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Verify Final Permissions ===')
stdin, stdout, stderr = ssh.exec_command(r'icacls C:\stores\learningtool\data\learningtool.db')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Restart App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
print('Restarted')

import time
time.sleep(3)

print('\n=== Test API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -o nul -w "%{http_code}" http://localhost:5192/api/auth/health')
status_code = stdout.read().decode('utf-8', errors='ignore').strip()
print(f'HTTP Status: {status_code}')

if status_code == '200':
    print('✓ SUCCESS - API is working!')
else:
    print(f'✗ FAIL - Still getting {status_code}')

ssh.close()
