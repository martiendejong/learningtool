#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Current www directory ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\www')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Check for /api application ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebApplication -Site LearningToolApp"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Current frontend directory ===')
stdin, stdout, stderr = ssh.exec_command('dir C:\\stores\\learningtool\\frontend')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
