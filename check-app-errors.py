#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Check if app pool is running
print('=== App Pool Status ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebAppPoolState LearningToolPool"')
print(stdout.read().decode('utf-8', errors='ignore'))

# Check Windows Event Log for ANY application errors (not just ASP.NET)
print('\n=== Recent Application Errors (All Sources) ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -EntryType Error -Newest 5 | Format-List TimeGenerated,Source,Message"')
output = stdout.read().decode('utf-8', errors='ignore')
if output.strip():
    print(output)
else:
    print('No errors found')

# Check if backend process is running
print('\n=== Backend Process ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Process -Name LearningTool.API -ErrorAction SilentlyContinue | Select-Object ProcessName,Id,StartTime"')
print(stdout.read().decode('utf-8', errors='ignore'))

# Try to read the FULL stdout log
print('\n=== Full Latest Stdout Log ===')
stdin, stdout, stderr = ssh.exec_command(r'powershell -Command "Get-ChildItem C:\stores\learningtool\backend\logs -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content"')
try:
    print(stdout.read().decode('utf-8', errors='replace')[-5000:])  # Last 5000 chars
except:
    print("Could not read log")

ssh.close()
