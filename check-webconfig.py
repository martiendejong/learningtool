#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== web.config content ===')
stdin, stdout, stderr = ssh.exec_command('type C:\\stores\\learningtool\\backend\\web.config')
config = stdout.read().decode('utf-8', errors='replace')
print(config)

ssh.close()
