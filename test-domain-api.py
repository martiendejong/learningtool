#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Testing API at https://learning.prospergenics.com/api ===')

# Test health endpoint
stdin, stdout, stderr = ssh.exec_command('curl -s -k -w "\\n\\nHTTP: %{http_code}\\n" https://learning.prospergenics.com/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(response)

# Also test if API responds to auth endpoints
print('\n=== Testing login endpoint (should return 400 for empty body) ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -k -w "\\nHTTP: %{http_code}\\n" -X POST https://learning.prospergenics.com/api/auth/login -H "Content-Type: application/json" -d "{}"')
login_response = stdout.read().decode('utf-8', errors='ignore')
print(login_response)

ssh.close()
