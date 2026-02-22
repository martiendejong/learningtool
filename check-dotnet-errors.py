#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== .NET Runtime Errors ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \\".NET Runtime\\" -EntryType Error -Newest 3 | Format-List TimeGenerated,Message"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== IIS W3SVC Errors ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName System -Source \\"W3SVC\\" -EntryType Error -Newest 3 -ErrorAction SilentlyContinue | Format-List TimeGenerated,Message"')
output = stdout.read().decode('utf-8', errors='ignore')
if output.strip():
    print(output)
else:
    print('No W3SVC errors')

ssh.close()
