#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import paramiko
import sys

# Set UTF-8 output
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Content of stdout_20260222013313_9436.log (928 bytes) ===')
stdin, stdout, stderr = ssh.exec_command('type C:\\stores\\learningtool\\backend\\logs\\stdout_20260222013313_9436.log')
log_content = stdout.read().decode('utf-8', errors='replace')
print(log_content)

ssh.close()
