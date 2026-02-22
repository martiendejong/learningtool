#!/usr/bin/env python3
import paramiko

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

# Stop app pool
print('=== Stopping App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Stop-WebAppPool LearningToolPool"')
stdout.read()
print('Stopped')

import time
time.sleep(2)

# Upload files
print('\n=== Uploading files ===')
sftp = ssh.open_sftp()

import os
local_dir = r'C:\Projects\learningtool\publish\backend'
remote_dir = r'C:\stores\learningtool\backend'

uploaded_count = 0
for root, dirs, files in os.walk(local_dir):
    for file in files:
        local_path = os.path.join(root, file)
        relative_path = os.path.relpath(local_path, local_dir)
        remote_path = os.path.join(remote_dir, relative_path).replace('\\', '/')

        try:
            sftp.put(local_path, remote_path)
            uploaded_count += 1
            if uploaded_count % 10 == 0:
                print(f'Uploaded {uploaded_count} files...')
        except Exception as e:
            print(f'Error uploading {relative_path}: {e}')

print(f'Total uploaded: {uploaded_count} files')
sftp.close()

# Start app pool
print('\n=== Starting App Pool ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Start-WebAppPool LearningToolPool"')
stdout.read()
print('Started')

time.sleep(5)

# Test
print('\n=== Testing API ===')
stdin, stdout, stderr = ssh.exec_command('curl -s http://localhost:5192/api/auth/health')
response = stdout.read().decode('utf-8', errors='ignore')

if 'healthy' in response.lower():
    print('✓ SUCCESS! API is healthy!')
    print(response)
elif response and len(response) < 300:
    print('Response:', response)
else:
    print('Got HTML error page. Checking logs...')
    stdin2, stdout2, stderr2 = ssh.exec_command(r'powershell -Command "Get-EventLog -LogName Application -Source \\".NET Runtime\\" -Newest 1 | Select-Object -ExpandProperty Message"')
    error = stdout2.read().decode('utf-8', errors='ignore')
    if 'line 152' in error or 'Migrate' in error:
        print('\nSTILL trying migrations - code change didnt work!')
    else:
        print('\nDifferent error:')
        print(error[:500])

ssh.close()
