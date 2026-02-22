#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== All recent errors (last 5 minutes) ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -EntryType Error -After (Get-Date).AddMinutes(-5) | Format-Table TimeGenerated,Source,Message -AutoSize"')
errors = stdout.read().decode('utf-8', errors='ignore')
if errors.strip():
    print(errors)
else:
    print('No errors in last 5 minutes!')

print('\n=== Test API with verbose curl ===')
stdin, stdout, stderr = ssh.exec_command('curl -v -k https://learning.prospergenics.com/api/auth/health 2>&1')
response = stdout.read().decode('utf-8', errors='ignore')
print(response[:1000])

ssh.close()
