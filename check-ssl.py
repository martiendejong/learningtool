#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Check SSL certificates ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-ChildItem Cert:\\LocalMachine\\My | Where-Object { $_.Subject -like \'*prospergenics*\' } | Format-List Subject,Thumbprint,NotAfter"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Check HTTPS binding configuration ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-WebBinding -Name LearningToolApp | Where-Object { $_.protocol -eq \'https\' } | Format-List *"')
print(stdout.read().decode('utf-8', errors='ignore'))

print('\n=== Quick fix: Disable HTTPS redirect temporarily ===')
print('Option 1: Remove UseHttpsRedirection from Program.cs')
print('Option 2: Configure SSL certificate')
print('Option 3: Test via HTTP by accessing http://learning.prospergenics.com/api directly')

ssh.close()
