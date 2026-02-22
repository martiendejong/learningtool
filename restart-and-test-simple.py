#!/usr/bin/env python3
import paramiko
import time

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== 1. Restart app pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Restart-WebAppPool -Name LearningToolPool"')
stdout.read()
print('App pool restarted')

print('\n=== 2. Wait for app to start ===')
time.sleep(5)

print('\n=== 3. Test health endpoint ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -k https://learning.prospergenics.com/api/auth/health')
health = stdout.read().decode('utf-8', errors='ignore')
print('Health response:', health if health else '(empty)')

print('\n=== 4. Test chat (if logged in user exists) ===')
# Just test that the endpoint responds
stdin, stdout, stderr = ssh.exec_command('curl -s -k -w "\nHTTP_CODE: %{http_code}" https://learning.prospergenics.com/api/chat/history?limit=1')
print(stdout.read().decode('utf-8', errors='ignore'))

ssh.close()
