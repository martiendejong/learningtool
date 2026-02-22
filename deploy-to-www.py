#!/usr/bin/env python3
import paramiko
import os

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Deploying frontend to www directory ===')
sftp = ssh.open_sftp()

local_dir = r'C:\Projects\learningtool\frontend\dist'
remote_dir = r'C:\stores\learningtool\www'

# Upload all files
uploaded = 0
for root, dirs, files in os.walk(local_dir):
    for dir in dirs:
        remote_subdir = os.path.join(remote_dir, os.path.relpath(os.path.join(root, dir), local_dir)).replace('\\', '/')
        try:
            sftp.mkdir(remote_subdir)
        except:
            pass  # Directory might already exist

    for file in files:
        local_path = os.path.join(root, file)
        relative_path = os.path.relpath(local_path, local_dir)
        remote_path = os.path.join(remote_dir, relative_path).replace('\\', '/')

        try:
            sftp.put(local_path, remote_path)
            uploaded += 1
            if uploaded % 5 == 0:
                print(f'Uploaded {uploaded} files...')
        except Exception as e:
            print(f'Error uploading {relative_path}: {e}')

print(f'\nTotal: {uploaded} files uploaded')
sftp.close()

print('\n=== Testing https://learning.prospergenics.com ===')
stdin, stdout, stderr = ssh.exec_command('curl -s -k https://learning.prospergenics.com 2>&1')
response = stdout.read().decode('utf-8', errors='ignore')

if 'index-' in response and '.js' in response:
    print('SUCCESS! Frontend is deployed and accessible')
    print('URL: https://learning.prospergenics.com')
elif '<!DOCTYPE' in response or '<html' in response:
    print('Frontend is serving HTML (good sign)')
    print('URL: https://learning.prospergenics.com')
else:
    print('Response:', response[:500])

ssh.close()
