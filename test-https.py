#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Test with HTTPS
stdin, stdout, stderr = ssh.exec_command('curl -s -k -w "\\n\\nHTTP_CODE: %{http_code}\\n" https://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')
print(response)

# Also try the actual frontend URL
print('\n\n=== Testing Frontend ===')
stdin2, stdout2, stderr2 = ssh.exec_command('curl -s -w "\\n\\nHTTP_CODE: %{http_code}\\n" http://85.215.217.154:5192')
frontend = stdout2.read().decode('utf-8', errors='ignore')
print(frontend[:1000] if len(frontend) > 1000 else frontend)

ssh.close()
