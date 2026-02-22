#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Website | Select-Object name,physicalPath,state,id | Format-Table -AutoSize"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Sites with port 5192 or 80 ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebBinding | Where-Object { $_.bindingInformation -like \'*:5192*\' -or $_.bindingInformation -like \'*learning*\' }"')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
