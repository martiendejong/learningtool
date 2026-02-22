#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. Find database file ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\*.db /s /b')
print(stdout.read().decode('utf-8', errors='ignore'))
err = stderr.read().decode('utf-8', errors='ignore')
if err:
    print('ERROR:', err)

print('\n=== 2. Check appsettings.json for connection string ===')
stdin, stdout, stderr = ssh.exec_command('type C:\\stores\\learningtool\\appsettings.json')
appsettings = stdout.read().decode('utf-8', errors='ignore')
print(appsettings)

print('\n=== 3. Check if sqlite3 is available ===')
stdin, stdout, stderr = ssh.exec_command('where sqlite3')
print(stdout.read().decode('utf-8', errors='ignore'))
err = stderr.read().decode('utf-8', errors='ignore')
if err:
    print('sqlite3 not found in PATH, ERROR:', err)

ssh.close()
