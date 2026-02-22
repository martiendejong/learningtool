#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Check File Attributes ===')
stdin, stdout, stderr = ssh.exec_command(r'attrib C:\stores\learningtool\data\learningtool.db')
attrs = stdout.read().decode('utf-8', errors='ignore')
print(attrs)

if 'R' in attrs:
    print('\n!!! FILE IS MARKED READONLY !!!')
    print('Removing readonly attribute...')
    stdin, stdout, stderr = ssh.exec_command(r'attrib -R C:\stores\learningtool\data\learningtool.db')
    print(stdout.read().decode('utf-8', errors='ignore'))

    print('\nVerifying...')
    stdin, stdout, stderr = ssh.exec_command(r'attrib C:\stores\learningtool\data\learningtool.db')
    print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Also check directory attribute ===')
stdin, stdout, stderr = ssh.exec_command(r'attrib C:\stores\learningtool\data')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Check connection string in appsettings.Production.json ===')
stdin, stdout, stderr = ssh.exec_command(r'type C:\stores\learningtool\backend\appsettings.Production.json')
config = stdout.read().decode('utf-8', errors='ignore')
print(config)

if 'Mode=ReadOnly' in config or 'mode=ReadOnly' in config:
    print('\n!!! CONNECTION STRING HAS ReadOnly MODE !!!')

print('\n=== Restart App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool LearningToolPool"')
print('Restarted')

import time
time.sleep(3)

print('\n=== Test API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -w "\\nHTTP Status: %{http_code}" http://localhost:5192/api/auth/health')
print(stdout.read().decode('utf-8', errors='ignore')[:500])

ssh.close()
