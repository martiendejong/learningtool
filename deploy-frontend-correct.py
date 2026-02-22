#!/usr/bin/env python3
import paramiko
import os

ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect('85.215.217.154', username='administrator', password=r'3WsXcFr$7YhNmKi*')

print('=== Uploading frontend files ===')
sftp = ssh.open_sftp()

local_dir = r'C:\Projects\learningtool\frontend\dist'
remote_dir = r'C:\stores\learningtool\www'  # CORRECTED: was 'frontend', should be 'www'

uploaded = 0
for root, dirs, files in os.walk(local_dir):
    for file in files:
        local_path = os.path.join(root, file)
        relative_path = os.path.relpath(local_path, local_dir)
        remote_path = os.path.join(remote_dir, relative_path).replace('\\', '/')

        # Create remote directory if needed
        remote_dir_path = os.path.dirname(remote_path)
        try:
            sftp.stat(remote_dir_path)
        except:
            try:
                sftp.mkdir(remote_dir_path)
            except:
                pass

        try:
            sftp.put(local_path, remote_path)
            uploaded += 1
            print('Uploaded: {}'.format(relative_path))
        except Exception as e:
            print('Error uploading {}: {}'.format(relative_path, e))

print('Uploaded {} files'.format(uploaded))
sftp.close()

print('\n=== Verifying deployment ===')
stdin, stdout, stderr = ssh.exec_command('powershell -Command "Get-Item C:\\stores\\learningtool\\www\\index.html | Select-Object LastWriteTime"')
output = stdout.read().decode('utf-8', errors='ignore')
print(output)

ssh.close()
print('\nFrontend deployed successfully!')
print('URL: https://learning.prospergenics.com')
