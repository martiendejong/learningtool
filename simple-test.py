#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Test health endpoint
stdin, stdout, stderr = ssh.exec_command('curl -s -w "\\n\\nHTTP_CODE: %{http_code}\\n" http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(response)

ssh.close()
