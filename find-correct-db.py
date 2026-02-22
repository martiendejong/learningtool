#!/usr/bin/env python3
import paramiko
import json

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. Check www directory structure ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\www')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== 2. Find appsettings.json in www ===')
stdin, stdout, stderr = ssh.exec_command('type C:\\stores\\learningtool\\www\\appsettings.json 2>&1')
appsettings = stdout.read().decode('utf-8', errors='ignore')
print(appsettings)

print('\n=== 3. Check backend directory ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\backend')
backend_dir = stdout.read().decode('utf-8', errors='ignore')
print(backend_dir)

print('\n=== 4. Find appsettings in backend ===')
stdin, stdout, stderr = ssh.exec_command('type C:\\stores\\learningtool\\backend\\appsettings.json 2>&1')
backend_settings = stdout.read().decode('utf-8', errors='ignore')
print(backend_settings)

ssh.close()
