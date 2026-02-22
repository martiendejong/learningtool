#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Current IIS Site Bindings ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Import-Module WebAdministration; Get-WebBinding | Format-Table protocol,bindingInformation -AutoSize"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Specific Site Bindings ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Import-Module WebAdministration; Get-WebBinding -Name \\"Default Web Site\\" | Format-Table protocol,bindingInformation"')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
