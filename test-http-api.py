#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Test HTTP API (port 80) ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print('Response:', response if response else 'Empty')

print('\n=== Test HTTP API with status code ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -w "\\nHTTP_CODE: %{http_code}\\n" -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response2 = stdout.read().decode('utf-8', errors='ignore')
print(response2)

print('\n=== Try from external domain ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -w "\\nHTTP_CODE: %{http_code}\\n" http://learning.prospergenics.com/api/auth/health')
external = stdout.read().decode('utf-8', errors='ignore')
print(external[:500] if external else 'Empty')

ssh.close()
