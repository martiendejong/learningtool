#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Latest stdout log (last 100 lines) ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 100"')
print(stdout.read().decode('utf-8', errors='replace'))

print('\n\n=== Latest Application Error ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -EntryType Error -Source \\"IIS AspNetCore Module V2\\" -Newest 1 | Select-Object -ExpandProperty Message"')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
