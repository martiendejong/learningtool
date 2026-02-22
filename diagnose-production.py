#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Check if database exists
print('=== Database Check ===')
stdin, stdout, stderr = ssh.exec_command(r'dir C:\stores\learningtool\data')
print(stdout.read().decode('utf-8', errors='ignore'))
err = stderr.read().decode('utf-8', errors='ignore')
if err:
    print('ERROR:', err)

# Check application event log for ASP.NET errors
print('\n=== ASP.NET Core Errors ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \\"ASP.NET Core*\\" -Newest 3 -ErrorAction SilentlyContinue | Format-List TimeGenerated,EntryType,Message"')
errors = stdout.read().decode('utf-8', errors='ignore')
if errors.strip():
    print(errors)
else:
    print('No ASP.NET Core errors found')

# Check stdout log file
print('\n=== Stdout Log ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 50"')
log = stdout.read().decode('utf-8', errors='ignore')
if log.strip():
    print(log)
else:
    print('No log files found')

# Try to get more specific error
print('\n=== IIS Request Details ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Content C:\\inetpub\\logs\\LogFiles\\W3SVC*\\*.log -Tail 10"')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
