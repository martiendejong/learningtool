#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Test with Host header
print('=== Test API with Host header ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print('Response:', response if response else 'Empty')

# Try the site root to see if it's even accessible
print('\n=== Test site root ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -H "Host: learning.prospergenics.com" http://localhost/')
root_response = stdout.read().decode('utf-8', errors='ignore')
print('Site root:', 'HTML found' if '<html' in root_response else root_response[:200])

# Check recent error logs
print('\n=== Recent errors ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -EntryType Error -Source \\"IIS AspNetCore Module V2\\" -Newest 1 | Select-Object -ExpandProperty Message"')
error = stdout.read().decode('utf-8', errors='ignore')
if error.strip():
    print(error[:500])
else:
    print('No recent errors')

ssh.close()
