#!/usr/bin/env python3
import paramiko
import time

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Stopping App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Stop-WebAppPool LearningToolPool"')
stdout.read()
print('Stopped')

time.sleep(3)

print('\n=== Starting App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Start-WebAppPool LearningToolPool"')
stdout.read()
print('Started')

time.sleep(5)

print('\n=== Test API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -k https://learning.prospergenics.com/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')

if response and len(response) < 100 and 'healthy' in response.lower():
    print('SUCCESS! API is healthy!')
    print(response)
elif response:
    print('Response:', response[:500])
else:
    print('No response')

    # Check latest error
    print('\n=== Latest Error ===')
    stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-EventLog -LogName Application -Source \\".NET Runtime\\" -Newest 1 | Select-Object TimeGenerated,@{Name=\\"Message\\";Expression={$_.Message.Substring(0,[Math]::Min(500,$_.Message.Length))}}"')
    error = stdout.read().decode('utf-8', errors='ignore')
    print(error)

ssh.close()
